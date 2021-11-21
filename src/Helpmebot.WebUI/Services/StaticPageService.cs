namespace Helpmebot.WebUI.Services
{
    using System.Collections.Generic;
    using System.IO;
    using Helpmebot.WebUI.Models;
    using Markdig;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public class StaticPageService : IStaticPageService
    {
        private MarkdownPipeline markdownPipeline;

        public StaticPageService()
        {
            this.markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter().Build();
        }

        void GetNavEntries()
        {
            
        }
        
        public StaticPage Load(string pageName)
        {
            var text = File.ReadAllText($"Pages/{pageName}.md");

            var yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Dictionary<string, string> frontMatter;
            using (var input = new StringReader(text))
            {
                var parser = new Parser(input);
                parser.Expect<StreamStart>();
                parser.Expect<DocumentStart>();
                frontMatter = yamlDeserializer.Deserialize<Dictionary<string,string>>(parser);
                parser.Expect<DocumentEnd>();
            }

            if (!frontMatter.ContainsKey("title") || !frontMatter.ContainsKey("path"))
            {
                return null;
            }
            
            var page = new StaticPage
            {
                Title = frontMatter["title"],
                Route = frontMatter["path"],
                Content = text,
                Pipeline = this.markdownPipeline
            };

            return page;
        }
    }
}