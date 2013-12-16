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

namespace helpmebot6.Commands
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Services.Interfaces;

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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Page(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            // TODO: link to basewiki
            Stream rawDataStream =
                HttpRequest.get(
                    "http://en.wikipedia.org/w/api.php?action=query&prop=revisions|info&rvprop=user|comment&redirects&inprop=protection&format=xml&titles="
                    + string.Join(" ", this.Arguments));

            XmlTextReader xtr = new XmlTextReader(rawDataStream);

            CommandResponseHandler crh = new CommandResponseHandler();

            string redirects = null;
            ArrayList protection = new ArrayList();
            string title;
            string size;
            string comment;
            DateTime touched = DateTime.MinValue;
            string user = title = comment = size = null;

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
                                return new CommandResponseHandler(this.MessageService.RetrieveMessage("pageMissing", this.Channel, null));
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
                crh.respond(this.MessageService.RetrieveMessage("pageRedirect", this.Channel, redirArgs));
            }

            string[] margs = { title, user, touched.ToString(), comment, size };
            crh.respond(this.MessageService.RetrieveMessage("pageMainResponse", this.Channel, margs));

            foreach (PageProtection p in protection)
            {
                string[] pargs =
                    {
                        title, p.Type, p.Level,
                        p.Expiry == DateTime.MaxValue ? "infinity" : p.Expiry.ToString()
                    };
                crh.respond(this.MessageService.RetrieveMessage("pageProtected", this.Channel, pargs));
            }

            return crh;
        }
    }
}
