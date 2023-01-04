namespace Helpmebot.AccountCreations.Model.Terraform
{
    using System.Text.Json.Serialization;

    public class RunNotification
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
        
        [JsonPropertyName("trigger")]
        public string Trigger { get; set; }
        
        [JsonPropertyName("run_status")]
        public string RunStatus { get; set; }
        
        [JsonPropertyName("run_updated_at")]
        public string RunUpdatedAt { get; set; }
        
        [JsonPropertyName("run_updated_by")]
        public string RunUpdatedBy { get; set; }
    }
}