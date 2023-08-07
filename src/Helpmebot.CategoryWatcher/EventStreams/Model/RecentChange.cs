namespace Helpmebot.CategoryWatcher.EventStreams.Model
{
    using Newtonsoft.Json;

    public class RecentChange
    {
        public string Title { get; set; }
        
        [JsonProperty("title_url")]
        public string TitleUrl { get; set; }
        
        [JsonProperty("notify_url")]
        public string NotifyUrl { get; set; }

        [JsonProperty("$schema")]
        public string Schema { get; set; }

        public string Type { get; set; }
        public bool? Bot { get; set; }
        public string Comment { get; set; }
        public int? Id { get; set; }
        public RecentChangeLength Length { get; set; }

        [JsonProperty("log_action")]
        public string LogAction { get; set; }

        [JsonProperty("log_action_comment")]
        public string LogActionComment { get; set; }

        [JsonProperty("log_id")]
        public int? LogId { get; set; }

        [JsonProperty("log_params")]
        public dynamic LogParams { get; set; }

        [JsonProperty("log_type")]
        public string LogType { get; set; }

        public RecentChangeMeta Meta { get; set; }
        public bool? Minor { get; set; }
        public int? Namespace { get; set; }
        public string ParsedComment { get; set; }
        public bool? Patrolled { get; set; }
        public RecentChangeRevision Revision { get; set; }

        [JsonProperty("server_name")]
        public string ServerName { get; set; }

        [JsonProperty("server_script_path")]
        public string ServerScriptPath { get; set; }

        [JsonProperty("server_url")]
        public string ServerUrl { get; set; }

        public int? Timestamp { get; set; }
        public string User { get; set; }
        public string Wiki { get; set; }
    }
}