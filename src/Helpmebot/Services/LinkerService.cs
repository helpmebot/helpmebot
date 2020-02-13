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
namespace Helpmebot.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.IrcClient.Events;

    /// <summary>
    ///     LinkerService and link parser
    /// </summary>
    public class LinkerService : ILinkerService
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly ISession databaseSession;
        private readonly Dictionary<string, string> lastLink;

        /// <summary>
        /// Initialises a new instance of the <see cref="LinkerService"/> class.
        /// </summary>
        public LinkerService(
            IMediaWikiApiHelper apiHelper,
            ISession databaseSession)
        {
            this.apiHelper = apiHelper;
            this.databaseSession = databaseSession;
            this.lastLink = new Dictionary<string, string>();
        }

        #region Public Methods and Operators

        public string ConvertWikilinkToUrl(string destination, string link)
        {
            var iwprefix = link.Split(':')[0];

            var prefix = this.databaseSession
                .CreateCriteria<InterwikiPrefix>()
                .Add(Restrictions.Eq("Prefix", iwprefix))
                .UniqueResult<InterwikiPrefix>();
            
            var url = prefix == null ? string.Empty : Encoding.UTF8.GetString(prefix.Url);

            var source = link;
            
            if (link.Split(':').Length == 1 || url == string.Empty)
            {
                url = this.GetWikiArticleBasePath(destination);
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

        private string GetWikiArticleBasePath(string destination)
        {
            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(destination);

            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            var url = mediaWikiApi.GetArticlePath();

            this.apiHelper.Release(mediaWikiApi);
            return url;
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

        /// <summary>
        /// The anti-space.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <remarks>
        /// FIXME: UrlEncode?
        /// </remarks>
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

        /// <summary>
        /// The IRC private message event.
        /// </summary>
        /// <param name="sender">
        /// The IRC client sending this event.
        /// </param>
        /// <param name="e">
        /// The event args from the IRC event
        /// </param>
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

            var channel = this.databaseSession.GetChannelObject(messageTarget);
            
            if (channel != null)
            {
                this.databaseSession.Refresh(channel);
                
                if (channel.AutoLink)
                {
                    var links = this.ParseMessageForLinks(string.Join(" ", e.Message))
                        .Select(x => this.ConvertWikilinkToUrl(messageTarget, x));
                
                    e.Client.SendMessage(messageTarget, string.Join(", ", links));
                }
            }
        }

        #endregion
    }
}