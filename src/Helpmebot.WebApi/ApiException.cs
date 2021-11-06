namespace Helpmebot.WebApi
{
    using System;

    public class ApiException : Exception
    {
        public string RpcStatus { get; }

        public ApiException(string rpcStatus)
        {
            this.RpcStatus = rpcStatus;
        }
        
        public ApiException(string rpcStatus, Exception innerException) : base(string.Empty, innerException)
        {
            this.RpcStatus = rpcStatus;
        }
    }
}