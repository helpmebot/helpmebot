namespace Helpmebot.Configuration
{
    using System.IO;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public class ConfigurationReader
    {
        public static T ReadConfiguration<T>(string fileName)
        {
            return new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize<T>(File.ReadAllText(fileName));
        }
    }
}