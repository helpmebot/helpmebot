namespace Helpmebot.WebUI.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;

    public class FlagsController : ControllerBase
    {
        public FlagsController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/flags")]
        public IActionResult Index()
        {
            var flagData = new Dictionary<string, List<CommandInfo>>();
            foreach (var commandInfo in this.ApiService.GetRegisteredCommands())
            {
                foreach (var flag in commandInfo.Flags)
                {
                    if (!flagData.ContainsKey(flag.Flag))
                    {
                        flagData.Add(flag.Flag, new List<CommandInfo>());
                    }

                    flagData[flag.Flag].Add(commandInfo);
                }
            }

            var flagHelp = this.ApiService.GetFlagHelp();

            var model = new Tuple<Dictionary<string, Tuple<string, string>>, Dictionary<string, List<CommandInfo>>>(flagHelp, flagData);
            return View(model);
        }

        [HttpGet("/flags/groups")]
        public IActionResult FlagGroups()
        {
            return this.View(this.ApiService.GetFlagGroups());
        }
    }
}