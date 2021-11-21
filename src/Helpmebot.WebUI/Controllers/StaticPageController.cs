namespace Helpmebot.WebUI.Controllers
{
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebUI.Services;
    using Microsoft.AspNetCore.Mvc;

    public class StaticPageController : ControllerBase
    {
        private readonly IStaticPageService staticPageService;

        public StaticPageController(IApiService apiService, IStaticPageService staticPageService) : base(apiService)
        {
            this.staticPageService = staticPageService;
        }
        
        [HttpGet("/privacy")]
        public IActionResult Privacy()
        {
            return this.Render("privacy");
        }
        
        private IActionResult Render(string fileName)
        {
            var staticPage = this.staticPageService.Load(fileName);

            if (staticPage == null)
            {
                return this.Redirect("/");
            }

            return this.View("StaticView", staticPage);
        }
    }
}