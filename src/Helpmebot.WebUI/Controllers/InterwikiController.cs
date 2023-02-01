namespace Helpmebot.WebUI.Controllers
{
    using System.Linq;
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
            this.ViewData["q"] = this.HttpContext.Request.Query["q"].ToString();

            return this.View(this.ApiService.GetInterwikiList().OrderBy(x => x.ImportedAs ?? x.Prefix).ToList());
        }
    }
}