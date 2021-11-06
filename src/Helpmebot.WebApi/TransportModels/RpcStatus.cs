namespace Helpmebot.WebApi.TransportModels
{
    public static class RpcStatus
    {
        public const string OK = "ok"; 
        
        public const string GENERAL_ERROR = "error"; 
        
        public const string INVALID_API_VERSION = "invalid api version"; 
        public const string NOT_ENOUGH_FRAMES = "not enough frames"; 
        public const string INVALID_AUTH = "invalid auth";
        public const string NOT_IMPLEMENTED = "not implemented";
        public const string PARAMETER_TYPE_INVALID = "parameter type invalid";
    }
}