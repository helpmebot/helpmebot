namespace Helpmebot.WebUI.Controllers
{
    using Helpmebot.CoreServices;
    using Helpmebot.WebApi.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    public class CreditsController : ControllerBase
    {
        public CreditsController(IApiService apiService) : base(apiService)
        {
        }

        public IActionResult Index()
        {
            return this.View(Credits.Authors);
        }
    }
}