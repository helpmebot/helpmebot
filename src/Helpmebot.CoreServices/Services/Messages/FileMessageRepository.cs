namespace Helpmebot.CoreServices.Services.Messages
{
    using System.Collections.Generic;
    using System.IO;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public class FileMessageRepository : ReadOnlyMessageRepository
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, List<List<string>>> strings = new Dictionary<string, List<List<string>>>();
        public FileMessageRepository(ILogger logger)
        {
            this.logger = logger;
            var fileNames = Directory.GetFiles("Messages/");
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            
            foreach (var file in fileNames)
            {
                try
                {
                    var data = deserializer.Deserialize<FileMessageStore>(File.ReadAllText(file));

                    if (data == null)
                    {
                        continue;
                    }

                    if (data.Format != 1)
                    {
                        logger.ErrorFormat("Unknown data format {0} in file {1}; skipping...", data.Format, file);
                        continue;
                    }

                    foreach (var kvp in data.Dataset)
                    {
                        this.strings.Add(kvp.Key, kvp.Value);
                    }
                }
                catch (YamlException ex)
                {
                    this.logger.ErrorFormat(ex, "Failed to load file {0} - {1}", file, ex.Message);
                }
            }
        }
        
        public override bool SupportsContext => false;
        
        public override List<List<string>> Get(string key, string contextType, string context)
        {
            if (this.strings.ContainsKey(key))
            {
                return this.strings[key];
            }

            return null;
        }
    }
}