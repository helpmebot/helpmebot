namespace Helpmebot.CoreServices.Startup
{
    using System;
    using Helpmebot.Configuration;
    using Stwalkerster.Bot.CommandLib.Commands.Interfaces;

    public class CommandOverrideMapEntryInflater
    {
        public void Inflate(CommandOverrideConfiguration config)
        {
            var commandInterface = typeof(ICommand);
            
            foreach (var entry in config.OverrideMap)
            {
                entry.CommandType = TypeResolver.GetType(entry.Type);
                if (entry.CommandType == null)
                {
                    throw new Exception($"Unable to locate type {entry.Type} in override map");
                }
                
                if (!commandInterface.IsAssignableFrom(entry.CommandType ))
                {
                    throw new Exception($"Unable to type {entry.CommandType} is not a bot command.");
                }
            }
        }
    }
}