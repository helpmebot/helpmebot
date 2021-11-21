namespace Helpmebot.WebUI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Helpmebot.WebUI.Models;
    using Markdig;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public class StaticPageService : IStaticPageService
    {
        private MarkdownPipeline markdownPipeline;

        private Dictionary<string, StaticPage> routeMap = new();

        public StaticPageService()
        {
            this.markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter().Build();

            var files = Directory.GetFiles("Pages/", "*.md");
            
            foreach (var file in files)
            {
                var staticPage = this.Load(file);
                this.routeMap.Add(staticPage.Route, staticPage);
            }
        }

        public List<StaticPage> GetNavEntries()
        {
            return this.routeMap.Values.Where(x => x.NavigationTitle != null).ToList();
        }
        
        private StaticPage Load(string fileName)
        {
            var text = File.ReadAllText(fileName);

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

            if (frontMatter.ContainsKey("icon"))
            {
                page.NavigationIcon = frontMatter["icon"];
            }

            if (frontMatter.ContainsKey("navigationTitle"))
            {
                page.NavigationTitle = frontMatter["navigationTitle"];
            }
            
            return page;
        }

        public StaticPage GetPage(string route)
        {
            if (this.routeMap.ContainsKey(route))
            {
                return this.routeMap[route];
            }

            throw new Exception("Page not found");
        }

        public bool Exists(string route)
        {
            return this.routeMap.ContainsKey(route);
        }
    }
}