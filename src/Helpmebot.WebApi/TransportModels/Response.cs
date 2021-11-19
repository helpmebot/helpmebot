namespace Helpmebot.WebApi.TransportModels
{
    using System.Collections.Generic;

    public class Response
    {
        public ResponseKey Key { get; set; }
        public List<List<string>> Responses { get; set; }
        public string Repository { get; set; }
    }
}