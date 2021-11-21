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
        
        public IActionResult Index(string route)
        {
            var staticPage = this.staticPageService.GetPage(route);

            if (staticPage == null)
            {
                return this.Redirect("/");
            }

            return this.View("StaticView", staticPage);
        }
    }
}