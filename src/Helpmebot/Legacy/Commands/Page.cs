// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Page.cs" company="Helpmebot Development Team">
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
// <summary>
//   Retrieves information on a specific page
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web;
using Helpmebot.Model;
using NHibernate.Util;

namespace helpmebot6.Commands
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services;
    using Helpmebot.Services.Interfaces;
    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Retrieves information on a specific page
    /// </summary>
    internal class Page : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Page"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Page(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var pageTitle = GetPageTitle();

            var uri = GetApiCallUri(pageTitle);

            using (Stream rawDataStream = HttpRequest.Get(uri).ToStream())
            {
                var xtr = new XmlTextReader(rawDataStream);

                var crh = new CommandResponseHandler();

                string redirects = null;
                var protection = new ArrayList();
                string title;
                string size;
                string comment;
                DateTime touched = DateTime.MinValue;
                string user = title = comment = size = null;

                var messageService = this.CommandServiceHelper.MessageService;
                while (!xtr.EOF)
                {
                    xtr.Read();
                    if (xtr.IsStartElement())
                    {
                        switch (xtr.Name)
                        {
                            case "r":
                                // redirect!
                                // <r from="Sausages" to="Sausage" />
                                redirects = xtr.GetAttribute("from");
                                break;
                            case "page":
                                if (xtr.GetAttribute("missing") != null)
                                {
                                    var msg = messageService.RetrieveMessage("pageMissing", this.Channel, null);
                                    return new CommandResponseHandler(msg);
                                }

                                title = xtr.GetAttribute("title");
                                touched = DateTime.Parse(xtr.GetAttribute("touched"));

                                break;
                            case "rev":
                                // user, comment
                                // <rev user="RjwilmsiBot" comment="..." />
                                user = xtr.GetAttribute("user");
                                comment = xtr.GetAttribute("comment");
                                break;
                            case "pr":
                                // protections  
                                // <pr type="edit" level="autoconfirmed" expiry="2010-06-30T18:36:52Z" />
                                string time = xtr.GetAttribute("expiry");
                                protection.Add(
                                    new PageProtection(
                                        xtr.GetAttribute("type"),
                                        xtr.GetAttribute("level"),
                                        time == "infinity" ? DateTime.MaxValue : DateTime.Parse(time)));
                                break;
                        }
                    }
                }

                if (redirects != null)
                {
                    string[] redirArgs = { redirects, title };
                    crh.Respond(messageService.RetrieveMessage("pageRedirect", this.Channel, redirArgs));
                }

                string[] margs = { title, user, touched.ToString(CultureInfo.InvariantCulture), comment, size };
                crh.Respond(messageService.RetrieveMessage("pageMainResponse", this.Channel, margs));

                foreach (PageProtection p in protection)
                {
                    string[] pargs =
                        {
                            title, p.Type, p.Level,
                            p.Expiry == DateTime.MaxValue ? "infinity" : p.Expiry.ToString()
                        };
                    crh.Respond(messageService.RetrieveMessage("pageProtected", this.Channel, pargs));
                }

                return crh;
            }
        }

        private string GetPageTitle()
        {
            // FIXME: ServiceLocator
            var linker = ServiceLocator.Current.GetInstance<ILinkerService>();

            var naiveTitle = string.Join(" ", this.Arguments);

            var parsedPageTitles = linker.ParseMessageForLinks(naiveTitle);

            if (parsedPageTitles.Count == 0)
            {
                return naiveTitle;
            }

            return (string)parsedPageTitles.First();
        }

        private string GetApiCallUri(string pageTitle)
        {
            var mediaWikiSite = this.GetLocalMediawikiSite();

            UriBuilder builder = new UriBuilder(mediaWikiSite.Api);
            var query = HttpUtility.ParseQueryString(
                "action=query&prop=revisions|info&rvprop=user|comment&redirects&inprop=protection&format=xml");
            query["titles"] = pageTitle;
            builder.Query = query.ToString();
            var uri = builder.Uri.ToString();
            return uri;
        }
    }
}
