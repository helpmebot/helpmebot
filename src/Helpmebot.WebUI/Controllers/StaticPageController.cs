namespace Helpmebot.WebUI.Controllers
{
    using System;
    using Helpmebot.WebApi.Services.Interfaces;
    using Markdig;
    using Microsoft.AspNetCore.Mvc;
    
    public class StaticPageController : ControllerBase
    {
        public StaticPageController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/privacy")]
        public IActionResult Privacy()
        {
            return this.Render("privacy");
        }

        private IActionResult Render(string pageName)
        {
            var text = System.IO.File.ReadAllText($"Pages/{pageName}.md");

            var title = text.Substring(0, text.IndexOf('\n'));
            var content = text.Substring(text.IndexOf('\n') + 1);

            var markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            return this.View("StaticView", new Tuple<string, string>(title, Markdown.ToHtml(content, markdownPipeline)));
        }
    }
}