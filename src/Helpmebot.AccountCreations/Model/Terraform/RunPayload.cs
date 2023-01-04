namespace Helpmebot.AccountCreations.Model.Terraform
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class RunPayload
    {
        [JsonPropertyName("payload_version")]
        public int PayloadVersion { get; set; }
        
        [JsonPropertyName("notification_configuration_id")]
        public string NotificationConfigurationId { get; set; }
        
        [JsonPropertyName("run_url")]
        public string RunUrl { get; set; }
        
        [JsonPropertyName("run_id")]
        public string RunId { get; set; }
        
        [JsonPropertyName("run_message")]
        public string RunMessage { get; set; }
        
        [JsonPropertyName("run_created_at")]
        public string RunCreatedAt { get; set; }
        
        [JsonPropertyName("run_created_by")]
        public string RunCreatedBy { get; set; }
        
        [JsonPropertyName("workspace_id")]
        public string WorkspaceId { get; set; }
        
        [JsonPropertyName("workspace_name")]
        public string WorkspaceName { get; set; }
        
        [JsonPropertyName("organization_name")]
        public string OrganizationName { get; set; }
        
        [JsonPropertyName("notifications")]
        public List<RunNotification> Notifications { get; set; }
    }
}