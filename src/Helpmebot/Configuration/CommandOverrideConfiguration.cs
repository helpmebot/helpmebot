namespace Helpmebot.Configuration
{
    using System;
    using System.Collections.Generic;
    
    public class CommandOverrideConfiguration
    {
        public IList<OverrideMapEntry> OverrideMap { get; private set; }

        public CommandOverrideConfiguration(IList<OverrideMapEntry> overrideMap)
        {
            this.OverrideMap = overrideMap;
        }

        public class OverrideMapEntry
        {
            public string Keyword { get; private set; }
            public string Channel { get; private set; }
            public Type Type { get; private set; }

            public OverrideMapEntry(string keyword, string channel, Type type)
            {
                this.Keyword = keyword;
                this.Channel = channel;
                this.Type = type;
            }
        }
    }
}