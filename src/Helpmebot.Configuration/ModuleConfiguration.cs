namespace Helpmebot.Configuration
{
    using System.Collections.Generic;
    
    public class ModuleConfiguration
    {
        public List<ModuleInfo> Modules { get; set; }

        public class ModuleInfo
        {
            public string Assembly { get; set; }
            public string CastleFile { get; set; }
            public string Configuration { get; set; }
            public List<string> StaticConfiguration { get; set; }
        }
    }
}