namespace Helpmebot.Configuration
{
    using System;
    using System.Collections.Generic;

    public class CommandOverrideConfiguration
    {
        public IList<OverrideMapEntry> OverrideMap { get; set; }

        public class OverrideMapEntry
        {
            public string Keyword { get; set; }
            public string Channel { get; set; }
            public string Type { get; set; }
            public Type CommandType { get; set; }
        }
    }
}