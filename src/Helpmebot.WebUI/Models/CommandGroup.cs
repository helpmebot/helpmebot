namespace Helpmebot.WebUI.Models
{
    using System.Collections.Generic;

    public class CommandGroup
    {
        private static Dictionary<string, CommandGroup> CommandGroups = new Dictionary<string, CommandGroup>
        {
            {"Default", new CommandGroup("Default", "Standard commands", "", priority: 5)},
            {"Diagnostics", new CommandGroup("Diagnostics", "Diagnostic commands", "Bot diagnostics and debugging tools", priority: 50)},
            {"Fun", new CommandGroup("Fun", "Fun commands", "Non-serious commands for non-serious channels", priority: 40)},
            {"Brain", new CommandGroup("Brain", "Brain commands", "Commands used to manage the bot's stored responses")},
            {"ACC", new CommandGroup("ACC", "ACC commands", "Commands used for Wikipedia's ACC process")},
        };

        public static CommandGroup GetGroup(string key)
        {
            if (CommandGroups.ContainsKey(key))
            {
                return CommandGroups[key];
            }

            return new CommandGroup(key, key, null);
        }

        public string Key { get; }
        public string Name { get; }
        public string Description { get; }
        public int Priority { get; }

        private CommandGroup(string key, string name, string description, int priority = 10)
        {
            this.Key = key;
            this.Name = name;
            this.Description = description;
            this.Priority = priority;
        }
    }
}