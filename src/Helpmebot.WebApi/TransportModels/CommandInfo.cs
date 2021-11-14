namespace Helpmebot.WebApi.TransportModels
{
    using System.Collections.Generic;

    public class CommandInfo
    {
        public string CanonicalName { get; set; }
        
        public string Type { get; set; }
        public List<string> Aliases { get; set; }
        public List<CommandFlag> Flags { get; set; } = new List<CommandFlag>();
        public string HelpCategory { get; set; }
        public string HelpSummary { get; set; }
        public List<SubcommandInfo> Subcommands { get; set; } = new List<SubcommandInfo>();
        
        public string ExtendedHelp { get; set; }

        public class SubcommandInfo
        {
            public string CanonicalName { get; set; }
            public List<string> Aliases { get; set; }
            public List<CommandFlag> Flags { get; set; } = new List<CommandFlag>();
            public List<string> Syntax { get; set; }
            public List<string> HelpText { get; set; }
        }

        public class CommandFlag
        {
            public string Flag { get; set; }
            public bool GlobalOnly { get; set; }
        }
    }
}