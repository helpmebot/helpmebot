// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System;
using System.Collections;
using System.IO;
using System.Xml;

#endregion

namespace helpmebot6.Commands
{
    internal class Page : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Stream rawDataStream =
                HttpRequest.get(
                    "http://en.wikipedia.org/w/api.php?action=query&prop=revisions|info&rvprop=user|comment&redirects&inprop=protection&format=xml&titles=" +
                    string.Join(" ", args));

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
                                return new CommandResponseHandler(Configuration.singleton().getMessage("pageMissing"));
                            }
                            // title, touched
                            // <page pageid="78056" ns="0" title="Sausage" touched="2010-05-23T17:46:16Z" lastrevid="363765722" counter="252" length="43232">
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
                            protection.Add(new PageProtection(xtr.GetAttribute("type"), xtr.GetAttribute("level"),
                                                              time == "infinity"
                                                                  ? DateTime.MaxValue
                                                                  : DateTime.Parse(time)));
                            break;
                        default:
                            break;
                    }
            }


            if (redirects != null)
            {
                string[] redirArgs = {redirects, title};
                crh.respond(Configuration.singleton().getMessage("pageRedirect", redirArgs));
            }

            string[] margs = {title, user, touched.ToString(), comment, size};
            crh.respond(Configuration.singleton().getMessage("pageMainResponse", margs));

            foreach (PageProtection p in protection)
            {
                string[] pargs = {
                                     title, p.type, p.level,
                                     p.expiry == DateTime.MaxValue ? "infinity" : p.expiry.ToString()
                                 };
                crh.respond(Configuration.singleton().getMessage("pageProtected", pargs));
            }

            return crh;
        }

        private struct PageProtection
        {
            public PageProtection(string type, string level, DateTime expiry)
            {
                this.type = type;
                this.level = level;
                this.expiry = expiry;
            }

            public readonly string type;
            public readonly string level;
            public DateTime expiry;
        }
    }
}