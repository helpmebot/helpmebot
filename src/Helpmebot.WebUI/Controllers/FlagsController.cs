namespace Helpmebot.WebUI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebUI.Models;

    public class FlagsController : ControllerBase
    {
        public FlagsController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/flags")]
        public IActionResult Index()
        {
            var flagData = new Dictionary<string, List<FlagCommandSummary>>();
            
            foreach (var commandInfo in this.ApiService.GetRegisteredCommands())
            {
                foreach (var flag in commandInfo.Flags)
                {
                    if (!flagData.ContainsKey(flag.Flag))
                    {
                        flagData.Add(flag.Flag, new List<FlagCommandSummary>());
                    }

                    flagData[flag.Flag].Add(new FlagCommandSummary
                    {
                        HelpText = commandInfo.HelpSummary,
                        ParentCommand = commandInfo.CanonicalName
                    });
                    
                }

                foreach (var subcommand in commandInfo.Subcommands)
                {
                    foreach (var flag in subcommand.Flags)
                    {
                        if (!flagData.ContainsKey(flag.Flag))
                        {
                            flagData.Add(flag.Flag, new List<FlagCommandSummary>());
                        }

                        flagData[flag.Flag].Add(new FlagCommandSummary
                        {
                            ParentCommand = commandInfo.CanonicalName,
                            Subcommand = subcommand.CanonicalName,
                            HelpText = subcommand.HelpText.FirstOrDefault()
                        });
                    }
                }
            }

            var flagHelp = this.ApiService.GetFlagHelp();

            var model = new Tuple<Dictionary<string, Tuple<string, string>>, Dictionary<string, List<FlagCommandSummary>>>(flagHelp, flagData);
            return View(model);
        }

        [HttpGet("/flags/groups")]
        public IActionResult FlagGroups()
        {
            var flagHelp = this.ApiService.GetFlagHelp();
            this.ViewData.Add("flagHelper", new FlagHelpHelper(flagHelp));
            
            return this.View(this.ApiService.GetFlagGroups());
        }
    }
}