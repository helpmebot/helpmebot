namespace Helpmebot.WebApi.Services
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Startup;
    using Helpmebot.WebApi.Configuration;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;
    using NetMQ;
    using NetMQ.Sockets;

    public class ApiTransportService : IApiTransportService, IDisposable
    {
        private readonly WebApiConfiguration configuration;
        private readonly IApiService api;
        private readonly ILogger logger;
        private readonly ResponseSocket server;
        private readonly Thread thread;
        private bool threadAlive;
        private readonly int expectedFrameCount = 5;
        private readonly string apiVersion = "0.1-alpha";

        public ApiTransportService(WebApiConfiguration configuration, IApiService api, ILogger logger)
        {
            this.configuration = configuration;
            this.api = api;
            this.logger = logger;
            this.server = new ResponseSocket();
            this.thread = new Thread(this.ThreadTask);
        }

        private void ThreadTask()
        {
            this.logger.Info("ZMQ transport thread started.");

            while (this.threadAlive)
            {
                // FRAME 0: API version
                // FRAME 1: Auth token
                // FRAME 2: Api Method
                // FRAME 3: Parameter type
                // FRAME 4: Parameter data
                
                // RESPONSE
                // FRAME 0: status
                // FRAME 1: response type
                // FRAME 2: response data
                
                var frames = this.server.ReceiveMultipartStrings(Encoding.UTF8, this.expectedFrameCount);
                this.logger.Trace("ZMQ message received");
                    
                try
                {
                    this.ValidateInboundMessage(frames[0], frames.Count);
                    this.ValidateAuth(frames[1]);

                    var methodInfo = this.api.GetType().GetMethod(frames[2]);
                    if (methodInfo == null)
                    {
                        throw new ApiException(RpcStatus.NOT_IMPLEMENTED);
                    }

                    var parameters = this.ParseParameter(methodInfo, frames[3], frames[4]);
                    
                    this.logger.Debug($"ZMQ: {methodInfo.Name}");
                    var result = methodInfo.Invoke(this.api, parameters);
                    this.logger.Trace($"ZMQ done {methodInfo.Name}");
                    
                    if (result != null)
                    {
                        var type = result.GetType();
                        var data = JsonSerializer.Serialize(result);

                        this.server.SendMoreFrame(RpcStatus.OK).SendMoreFrame(type.ToString()).SendFrame(data);
                    }
                    else
                    {
                        this.server.SendMoreFrame(RpcStatus.OK).SendMoreFrameEmpty().SendFrameEmpty();
                    }
                }
                catch (ApiException ex)
                {
                    this.logger.Warn($"ZMQ message error encountered - {ex.RpcStatus}", ex);
                    this.server.SendMoreFrame(ex.RpcStatus).SendMoreFrameEmpty().SendFrameEmpty();
                }
                catch (Exception ex)
                {
                    this.logger.Error($"ZMQ transport exception handled", ex);
                    this.server.SendMoreFrame(RpcStatus.GENERAL_ERROR).SendMoreFrameEmpty().SendFrameEmpty();
                }
                    
                this.logger.Trace($"ZMQ end of message");
            }
        }

        private object[] ParseParameter(MethodInfo methodInfo, string type, string data)
        {
            var parameterInfos = methodInfo.GetParameters();
            if (!parameterInfos.Any())
            {
                return Array.Empty<object>();
            }

            var parameterInfo = parameterInfos.First();

            if (string.IsNullOrWhiteSpace(type)
                || string.IsNullOrWhiteSpace(data)
                || parameterInfo.ParameterType != TypeResolver.GetType(type))
            {
                throw new ApiException(RpcStatus.PARAMETER_TYPE_INVALID);
            }
            
            object deserialize;
            try
            {
                deserialize = JsonSerializer.Deserialize(data, parameterInfo.ParameterType);
            }
            catch (JsonException ex)
            {
                throw new ApiException(RpcStatus.PARAMETER_TYPE_INVALID, ex);
            }

            return new[] { deserialize };
        }

        private void ValidateAuth(string token)
        {
            if (token != this.configuration.SystemToken)
            {
                throw new ApiException(RpcStatus.INVALID_AUTH);
            }
        }

        private void ValidateInboundMessage(string versionFrame, int frameCount)
        {
            if (versionFrame != this.apiVersion)
            {
                throw new ApiException(RpcStatus.INVALID_API_VERSION);
            }

            if (frameCount != this.expectedFrameCount)
            {
                throw new ApiException(RpcStatus.NOT_ENOUGH_FRAMES);
            }
        }

        public void Start()
        {
            this.server.Bind(this.configuration.BindAddress);
            this.threadAlive = true;
            this.thread.Start();
            this.logger.Info("ZMQ transport started");
        }

        public void Stop()
        {
            this.logger.Trace("Stopping ZMQ transport");
            this.threadAlive = false;
            this.thread.Join(3000);
            
            if (this.thread.IsAlive)
            {
                this.thread.Abort();
            }
            
            this.server.Unbind(this.configuration.BindAddress);
            this.logger.Info("Stopped ZMQ transport");

        }

        public void Dispose()
        {
            this.server?.Dispose();
            NetMQConfig.Cleanup();
        }
    }
}