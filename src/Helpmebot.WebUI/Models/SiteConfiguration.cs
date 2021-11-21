namespace Helpmebot.WebUI.Models
{
    public class SiteConfiguration
    {
        public string SystemApiToken { get; set; }
        public string ApiPath { get; set; }
        
        public bool AllowLogin { get; set; }
    }
}