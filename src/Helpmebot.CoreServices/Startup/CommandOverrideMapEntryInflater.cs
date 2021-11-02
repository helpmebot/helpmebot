namespace Helpmebot.CoreServices.Startup
{
    using System;
    using Castle.MicroKernel.SubSystems.Conversion;
    using Helpmebot.Configuration;

    public class CommandOverrideMapEntryInflater
    {
        private readonly IConversionManager conversionManager;

        public CommandOverrideMapEntryInflater(IConversionManager conversionManager)
        {
            this.conversionManager = conversionManager;
        }

        public void Inflate(CommandOverrideConfiguration config)
        {
            foreach (var entry in config.OverrideMap)
            {
                var entryCommandType = this.conversionManager.PerformConversion<Type>(entry.Type);
                entry.CommandType = entryCommandType;
            }
        }
    }
}