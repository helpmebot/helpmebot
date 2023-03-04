// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinkerService.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.CoreServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;

    /// <summary>
    ///     LinkerService and link parser
    /// </summary>
    public class LinkerService : ILinkerService
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly ISession databaseSession;
        private readonly IIrcClient client;
        private readonly ILogger logger;
        private readonly IChannelManagementService channelManagementService;
        private readonly Dictionary<string, string> lastLink;
        private readonly Dictionary<string, string> articlePathCache;

        /// <summary>
        /// Initialises a new instance of the <see cref="LinkerService"/> class.
        /// </summary>
        public LinkerService(
            IMediaWikiApiHelper apiHelper,
            ISession databaseSession,
            IIrcClient client,
            ILogger logger,
            IChannelManagementService channelManagementService)
        {
            this.apiHelper = apiHelper;
            this.databaseSession = databaseSession;
            this.client = client;
            this.logger = logger;
            this.channelManagementService = channelManagementService;
            this.articlePathCache = new Dictionary<string, string>();
            this.lastLink = new Dictionary<string, string>();
        }

        #region Public Methods and Operators

        public string ConvertWikilinkToUrl(string destination, string link)
        {
            return this.ConvertWikilinkToUrl(() => this.GetWikiArticleBasePath(destination), link);
        }
       
        public string ConvertWikilinkToUrl(Func<string> getWikiBasePath, string link)
        {
            var iwprefix = link.Split(':')[0];

            var prefix = this.databaseSession.QueryOver<InterwikiPrefix>()
                .Where(x => x.Prefix == iwprefix && x.ImportedAs == null)
                .List()
                .FirstOrDefault();
            
            var url = prefix == null ? string.Empty : prefix.Url;

            var source = link;
            
            if (link.Split(':').Length == 1 || url == string.Empty)
            {
                url = getWikiBasePath();
            }
            else
            {
                source = string.Join(":", link.Split(':'), 1, link.Split(':').Length - 1);
            }

            var resultString = url.Replace("$1", this.Antispace(source));

            if (resultString.StartsWith("//"))
            {
                resultString = "https:" + resultString;
            }
            
            return resultString;
        }

        public string GetLastLinkForChannel(string destination)
        {
            string lastLinkedLine;
            var success = this.lastLink.TryGetValue(destination, out lastLinkedLine);
            if (!success)
            {
                return string.Empty;
            }

            var links = this.ParseMessageForLinks(lastLinkedLine);

            return links.Aggregate(
                string.Empty,
                (current, link) => current + " " + this.ConvertWikilinkToUrl(destination, link)).TrimStart(' ');
        }

        public IList<string> ParseMessageForLinks(string message)
        {
            var newLinks = new List<string>();

            var linkRegex = new Regex(@"\[\[([^\[\]\|]*)(?:\]\]|\|)|{{(?:subst:)?([^{}\|]*)(?:}}|\|)");
            Match m = linkRegex.Match(message);
            while (m.Length > 0)
            {
                if (m.Groups[1].Length > 0)
                {
                    newLinks.Add(m.Groups[1].Value);
                }

                if (m.Groups[2].Length > 0)
                {
                    newLinks.Add("Template:" + m.Groups[2].Value);
                }

                m = m.NextMatch();
            }

            return newLinks;
        }
 
        #endregion

        #region Methods
        
        private string GetWikiArticleBasePath(string destination)
        {
            lock (this.articlePathCache)
            {
                if (this.articlePathCache.ContainsKey(destination))
                {
                    return this.articlePathCache[destination];
                }
            }

            var mediaWikiSite = this.channelManagementService.GetBaseWiki(destination);

            IMediaWikiApi mediaWikiApi = null;
            string url;
            
            try
            {
                mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
                url = mediaWikiApi.GetArticlePath();
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }

            lock (this.articlePathCache)
            {
                if (this.articlePathCache.ContainsKey(destination))
                {
                    this.articlePathCache.Remove(destination);
                }

                this.articlePathCache.Add(destination, url);
            }

            return url;
        }
        
        private string Antispace(string source)
        {
            return source.Replace(' ', '_')
                .Replace("%", "%25")
                .Replace("!", "%21")
                .Replace("*", "%2A")
                .Replace("'", "%27")
                .Replace("(", "%28")
                .Replace(")", "%29")
                .Replace(";", "%3B")
                
                // .Replace(":", "%3A")
                .Replace("@", "%40")
                .Replace("&", "%26")
                .Replace("=", "%3D")
                .Replace("+", "%2B")
                .Replace("$", "%24")
                .Replace(",", "%2C")
                
                // .Replace("/", "%2F")
                .Replace("?", "%3F").Replace("#", "%23").Replace("[", "%5B").Replace("]", "%5D");
        }

        #endregion
        
        public void IrcPrivateMessageEvent(object sender, MessageReceivedEventArgs e)
        {
            var newLink = this.ParseMessageForLinks(e.Message);
            if (newLink.Count == 0)
            {
                return;
            }

            var messageTarget = e.Target;
            if (messageTarget == e.Client.Nickname)
            {
                messageTarget = e.User.Nickname;
            }
                
            if (this.lastLink.ContainsKey(messageTarget))
            {
                this.lastLink.Remove(messageTarget);
            }

            this.lastLink.Add(messageTarget, e.Message);

            if (this.channelManagementService.AutoLinkEnabled(messageTarget))
            {
                var links = this.ParseMessageForLinks(string.Join(" ", e.Message))
                    .Select(x => this.ConvertWikilinkToUrl(messageTarget, x));
            
                e.Client.SendMessage(messageTarget, string.Join(", ", links));
            }
        }
        
        public void Start()
        {
            this.logger.Debug("Starting linker service");
            this.client.ReceivedMessage += this.IrcPrivateMessageEvent;
        }

        public void Stop()
        {
            this.logger.Debug("Stopping linker service");
            this.client.ReceivedMessage += this.IrcPrivateMessageEvent;
        }
    }
}