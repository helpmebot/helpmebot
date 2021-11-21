namespace Helpmebot.WebUI.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;

    public class StaticPageValueTransformer : DynamicRouteValueTransformer
    {
        private readonly IStaticPageService staticPageService;

        public StaticPageValueTransformer(IStaticPageService staticPageService)
        {
            this.staticPageService = staticPageService;
        }
        
        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (values.ContainsKey("pagename") && values["pagename"] != null)
            {
                if (this.staticPageService.Exists(Convert.ToString(values["pagename"])))
                {
                    values["controller"] = "StaticPage";
                    values["action"] = "Index";
                    values["route"] = values["pagename"];
                }
            }

            return values;
        }
    }
}