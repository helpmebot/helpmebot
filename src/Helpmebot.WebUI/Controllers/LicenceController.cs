using Microsoft.AspNetCore.Mvc;

namespace Helpmebot.WebUI.Controllers
{
    using System.Collections.Generic;
    using Helpmebot.WebApi.Services.Interfaces;

    public class LicenceController : ControllerBase
    {
        public LicenceController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/licence")]
        public IActionResult Index()
        {
            return View();
        }
    }
}