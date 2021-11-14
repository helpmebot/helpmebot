namespace Helpmebot.WebUI.Controllers
{
    using Helpmebot.WebApi.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    public class InterwikiController: ControllerBase
    {
        public InterwikiController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/interwiki")]
        public IActionResult Index()
        {
            return this.View(this.ApiService.GetInterwikiList());
        }
    }
}