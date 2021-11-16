namespace Helpmebot.CoreServices.Services.Messages
{
    using System.Collections.Generic;
    using System.IO;
    using YamlDotNet.Serialization;

    public class FileMessageRepository : ReadOnlyMessageRepository
    {
        private readonly Dictionary<string, List<List<string>>> strings = new Dictionary<string, List<List<string>>>();
        public FileMessageRepository()
        {
            var fileNames = Directory.GetFiles("Messages/");
            var deserializer = new DeserializerBuilder().Build();
            
            foreach (var file in fileNames)
            {
                var data = deserializer.Deserialize<Dictionary<string,List<List<string>>>>(File.ReadAllText(file));

                if (data == null)
                {
                    continue;
                } 
                
                foreach (var kvp in data)
                {
                    this.strings.Add(kvp.Key, kvp.Value);
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