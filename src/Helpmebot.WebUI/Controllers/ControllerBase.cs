namespace Helpmebot.WebUI.Controllers
{
    using Helpmebot.WebApi.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public abstract class ControllerBase : Controller
    {
        protected IApiService ApiService { get; }

        public ControllerBase(IApiService apiService)
        {
            this.ApiService = apiService;
        }
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this.ViewBag.BotStatus = this.ApiService.GetBotStatus();
            base.OnActionExecuting(context);
        }
    }
}