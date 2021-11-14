namespace Helpmebot.WebUI.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Helpmebot.WebApi.Services.Interfaces;

    public class FlagsController : ControllerBase
    {
        public FlagsController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/flags")]
        public IActionResult Index()
        {
            return View(this.ApiService.GetFlagHelp());
        }

        [HttpGet("/flags/groups")]
        public IActionResult FlagGroups()
        {
            return this.View(this.ApiService.GetFlagGroups());
        }
    }
}