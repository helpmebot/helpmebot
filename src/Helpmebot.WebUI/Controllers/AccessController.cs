namespace Helpmebot.WebUI.Controllers
{
    using System;
    using Helpmebot.WebApi.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    public class AccessController : ControllerBase
    {
        public AccessController(IApiService apiService) : base(apiService)
        {
        }

        [HttpGet("/access")]
        public IActionResult Index()
        {
            this.ViewData["q"] = this.HttpContext.Request.Query["q"].ToString();

            return this.View(this.ApiService.GetAccessControlList());
        }
    }
}