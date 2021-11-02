namespace Helpmebot.Configuration
{
    using System.Collections.Generic;
    
    public class ModuleConfiguration
    {
        public string Assembly { get; set; }
        public string CastleFile { get; set; }
        public List<ConfigurationFile> Configuration { get; set; }

        public class ConfigurationFile
        {
            public string File { get; set; }
            public string Type { get; set; }
        }
    }
}