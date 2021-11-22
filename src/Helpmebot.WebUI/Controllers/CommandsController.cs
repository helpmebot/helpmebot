using Microsoft.AspNetCore.Mvc;

namespace Helpmebot.WebUI.Controllers
{
    using System.Collections.Generic;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;

    public class CommandsController : ControllerBase
    {
        public CommandsController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/commands")]
        public IActionResult Index()
        {
            var flagHelp = this.ApiService.GetFlagHelp();
            this.ViewData.Add("flagHelper", new FlagHelpHelper(flagHelp));
            
            var registeredCommands = this.ApiService.GetRegisteredCommands();

            var groups = new Dictionary<string, List<CommandInfo>>();
            foreach (var command in registeredCommands)
            {
                if (!groups.ContainsKey(command.HelpCategory))
                {
                    groups.Add(command.HelpCategory, new List<CommandInfo>());
                }
                
                groups[command.HelpCategory].Add(command);
            }
            
            return View(groups);
        }
    }
}