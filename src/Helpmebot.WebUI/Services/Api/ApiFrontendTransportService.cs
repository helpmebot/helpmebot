namespace Helpmebot.WebUI.Services.Api
{
    using System;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using Helpmebot.CoreServices.Startup;
    using Helpmebot.WebApi.TransportModels;
    using NetMQ;
    using NetMQ.Sockets;

    public class ApiFrontendTransportService : IApiFrontendTransportService
    {
        private string connectPath = "tcp://localhost:5656";
        private string systemToken = "123456abcdef";       
        private readonly string apiVersion = "0.1-alpha";

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
            
            var frames = socket.ReceiveMultipartStrings(Encoding.UTF8, 3);

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
    }
}