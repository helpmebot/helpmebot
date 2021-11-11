namespace Helpmebot.WebUI.Controllers
{
    using System;
    using System.Diagnostics;
    using Helpmebot.WebUI.Models;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Mvc;

    public class ErrorController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("/error")]
        public IActionResult Error()
        {
            var httpContextFeature = (IExceptionHandlerFeature)this.HttpContext.Features[typeof(IExceptionHandlerFeature)];

            if (httpContextFeature.Error is TimeoutException)
            {
                HttpContext.Response.StatusCode = 503;
                return View("TimeoutException");
            }

            
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}