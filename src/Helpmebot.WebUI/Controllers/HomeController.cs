using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Helpmebot.WebUI.Models;

namespace Helpmebot.WebUI.Controllers
{
    using Helpmebot.WebApi.Services.Interfaces;
    using Microsoft.AspNetCore.Diagnostics;

    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger, IApiService apiService) : base(apiService)
        {
            this.logger = logger;
        }

        [HttpGet("/home")]
        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}