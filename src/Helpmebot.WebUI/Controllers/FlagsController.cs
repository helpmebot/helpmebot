using Microsoft.AspNetCore.Mvc;

namespace Helpmebot.WebUI.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.WebApi.Services.Interfaces;

    public class FlagsController : ControllerBase
    {
        public FlagsController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/flags")]
        public IActionResult Index()
        {
            var data = new Dictionary<string, (string, string)>();
            foreach (var fieldInfo in typeof(Flags).GetFields().Where(x => x.IsLiteral && x.FieldType == typeof(string)))
            {
                var flagHelpAttr = fieldInfo.GetCustomAttributes(typeof(FlagHelpAttribute), false).Cast<FlagHelpAttribute>().FirstOrDefault();
                var rawConstantValue = fieldInfo.GetRawConstantValue();
                if (flagHelpAttr == null || rawConstantValue == null)
                {
                    continue;
                }
            
                data.Add((string)rawConstantValue, (flagHelpAttr.QuickHelpText, flagHelpAttr.DetailedHelp));
            }
            
            return View(data);
        }
    }
}