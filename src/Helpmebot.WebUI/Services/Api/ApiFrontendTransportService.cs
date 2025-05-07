namespace Helpmebot.WebUI.Services.Api
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using Helpmebot.CoreServices.Startup;
    using Helpmebot.WebApi.TransportModels;
    using Helpmebot.WebUI.Models;
    using NetMQ;
    using NetMQ.Sockets;

    public class ApiFrontendTransportService : IApiFrontendTransportService
    {
        private readonly string connectPath;
        private readonly string systemToken;       
        private readonly string apiVersion = "0.1-alpha";
        private readonly int apiTimeoutSeconds;

        public ApiFrontendTransportService(SiteConfiguration configuration)
        {
            this.systemToken = configuration.SystemApiToken;
            this.connectPath = configuration.ApiPath;
            this.apiTimeoutSeconds = configuration.ApiTimeoutSeconds;
        }
        
        public TResponse RemoteProcedureCall<TParam, TResponse>(MethodBase method, TParam parameter)
        {
            using var socket = new RequestSocket();
            socket.Connect(this.connectPath);
            
            if (parameter != null)
            {
                socket
                    .SendMoreFrame(this.apiVersion)
                    .SendMoreFrame(this.systemToken)
                    .SendMoreFrame(method.Name)
                    .SendMoreFrame(parameter.GetType().FullName)
                    .SendFrame(JsonSerializer.Serialize(parameter));
            }
            else
            {
                socket
                    .SendMoreFrame(this.apiVersion)
                    .SendMoreFrame(this.systemToken)
                    .SendMoreFrame(method.Name)
                    .SendMoreFrameEmpty()
                    .SendFrameEmpty();
            }

            var frames = new List<string>();

            if (socket.TryReceiveMultipartStrings(new TimeSpan(0,0,this.apiTimeoutSeconds), Encoding.UTF8, ref frames))
            {

                if (frames[0] != RpcStatus.OK)
                {
                    throw new Exception($"RPC call failed: {frames[0]}");
                }

                if (typeof(TResponse) != TypeResolver.GetType(frames[1]))
                {
                    throw new Exception($"RPC call failed: type mismatch");
                }

                socket.Disconnect(this.connectPath);

                return JsonSerializer.Deserialize<TResponse>(frames[2]);
            }
            else
            {
                throw new TimeoutException();
            }
            
        }
    }
}