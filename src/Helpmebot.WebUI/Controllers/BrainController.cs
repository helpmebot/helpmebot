namespace Helpmebot.WebUI.Controllers
{
    using System.Linq;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebUI.Models;
    using Microsoft.AspNetCore.Mvc;

    public class BrainController : ControllerBase
    {
        public BrainController(IApiService apiService) : base(apiService)
        {
        }

        [HttpGet("/brain")]
        public IActionResult Index()
        {
            var brainItems = this.ApiService.GetBrainItems()
                .Select(x => new ExtendedBrainItem(x))
                .OrderBy(x => x.Keyword)
                .ToList();

            foreach (var brainItem in brainItems)
            {
                brainItem.Parse();
            }
            
            return this.View(brainItems);
        }
    }
}