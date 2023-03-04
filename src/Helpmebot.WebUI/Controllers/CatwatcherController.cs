namespace Helpmebot.WebUI.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Helpmebot.WebApi.Services.Interfaces;

    public class CatwatcherController : ControllerBase
    {
        public CatwatcherController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/catwatchers")]
        public IActionResult Index()
        {
            var catWatcherStatusList = this.ApiService.GetCatWatchers();
            return View(catWatcherStatusList);
        }
    }
}