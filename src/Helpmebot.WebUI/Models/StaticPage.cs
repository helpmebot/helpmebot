namespace Helpmebot.WebUI.Models
{
    using Markdig;

    public class StaticPage
    {
        public string Title { get; init; }
        public string Route { get; init; }
        public string Content { get; init; }
        public MarkdownPipeline Pipeline { get; init; }
        
        
        public string NavigationIcon { get; set; }
        public string NavigationTitle { get; set; }
        
        public string Html => Markdown.ToHtml(this.Content, this.Pipeline);
    }
}