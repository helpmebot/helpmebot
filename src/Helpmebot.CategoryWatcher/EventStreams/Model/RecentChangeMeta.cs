namespace Helpmebot.CategoryWatcher.EventStreams.Model
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    public class RecentChangeMeta
    {
        public string Domain { get; set; }
        [JsonProperty("dt")] private string dt;
        [JsonIgnore]
        public DateTime DateTime
        {
            get
            {
                return System.DateTime.Parse(this.dt, null, DateTimeStyles.AdjustToUniversal);
            }
        }
        public string Id { get; set; }
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        public string Stream { get; set; }
        public string Uri { get; set; }
        public string Topic { get; set; }
        public long? Partition { get; set; }
        public long? Offset { get; set; }
    }
}