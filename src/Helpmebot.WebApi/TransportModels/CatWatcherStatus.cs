namespace Helpmebot.WebApi.TransportModels
{
    using System;
    using System.Collections.Generic;

    public class CatWatcherStatus
    {
        public string Keyword { get; set; }
        public string Category { get; set; }
        public string Link { get; set; }
        public List<CatWatcherItemStatus> Items { get; set; }

        public class CatWatcherItemStatus
        {
            public string Page { get; set; }
            public string Link { get; set; }
            public DateTime WaitingSince { get; set; }
        }
    }
}