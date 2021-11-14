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
            return this.View(this.ApiService.GetAccessControlList());
        }
    }
}