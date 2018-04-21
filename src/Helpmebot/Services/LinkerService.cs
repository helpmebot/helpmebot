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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Events;

    /// <summary>
    ///     LinkerService and link parser
    /// </summary>
    public class LinkerService : ILinkerService
    {
        #region Fields

        /// <summary>
        /// The _last link.
        /// </summary>
        private readonly Dictionary<string, string> lastLink;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="LinkerService"/> class.
        /// </summary>
        public LinkerService()
        {
            this.lastLink = new Dictionary<string, string>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the real link.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="link">
        /// The link.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetRealLink(string destination, string link)
        {
            string iwprefix = link.Split(':')[0];

            // FIXME: servicelocator call - iwprefix repo
            var interwikiPrefixRepository = ServiceLocator.Current.GetInstance<IInterwikiPrefixRepository>();
            var prefix = interwikiPrefixRepository.GetByPrefix(iwprefix);

            string url = prefix == null ? string.Empty : Encoding.UTF8.GetString(prefix.Url);

            if (link.Split(':').Length == 1 || url == string.Empty)
            {
                url = LegacyConfig.Singleton()["wikiSecureUrl", destination];
                return url + Antispace(link);
            }

            return url.Replace("$1", Antispace(string.Join(":", link.Split(':'), 1, link.Split(':').Length - 1)));
        }

        /// <summary>
        /// The instance.
        /// </summary>
        /// <returns>
        /// The <see cref="ILinkerService"/>.
        /// </returns>
        [Obsolete]
        public static ILinkerService Instance()
        {
            return ServiceLocator.Current.GetInstance<ILinkerService>();
        }

        /// <summary>
        /// Gets the link.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetLink(string destination)
        {
            string lastLinkedLine;
            bool success = this.lastLink.TryGetValue(destination, out lastLinkedLine);
            if (!success)
            {
                return string.Empty;
            }

            ArrayList links = this.ReallyParseMessage(lastLinkedLine);

            return links.Cast<string>()
                .Aggregate(string.Empty, (current, link) => current + " " + GetRealLink(destination, link));
        }

        /// <summary>
        /// Really parses the message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="ArrayList"/>.
        /// </returns>
        public ArrayList ReallyParseMessage(string message)
        {
            var newLinks = new ArrayList();

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
        private static string Antispace(string source)
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
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public void IrcPrivateMessageEvent(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Command != "PRIVMSG" && e.Message.Command != "NOTICE")
            {
                return;
            }

            var parameters = e.Message.Parameters.ToList();
            var message = parameters[1];
            var channel = parameters[0];
                
            var newLink = this.ReallyParseMessage(message);
            if (newLink.Count == 0)
            {
                return;
            }

            if (this.lastLink.ContainsKey(channel))
            {
                this.lastLink.Remove(channel);
            }

            this.lastLink.Add(channel, message);
            
            if (LegacyConfig.Singleton()["autoLink", channel] == "true")
            {
                e.Client.SendMessage(channel, this.GetLink(message));
            }
        }

        #endregion
    }
}