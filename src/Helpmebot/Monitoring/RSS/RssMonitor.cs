// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RssMonitor.cs" company="Helpmebot Development Team">
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
//   Defines the RssMonitor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Monitoring.RSS
{
    using System;
    using System.Net;
    using System.Xml;
    using System.Xml.XPath;

    using Helpmebot.Threading;

    class RssMonitor : IThreadedSystem
    {
        public RssMonitor()
        {
                
        }

      //private Thread _t = new Thread(new ThreadStart(execute));

        string feedurl = "https://jira.toolserver.org/plugins/servlet/streams?key=ACC";

        void execute()
        {
            

            HttpWebRequest wreq = WebRequest.Create(this.feedurl) as HttpWebRequest;
            HttpWebResponse wrsp = wreq.GetResponse() as HttpWebResponse;


            XPathDocument xpd = new XPathDocument(wrsp.GetResponseStream());

            XPathNavigator xpn = xpd.CreateNavigator();
            XmlNamespaceManager xnm = new XmlNamespaceManager(xpn.NameTable);
            xnm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            xnm.AddNamespace("thr", "http://purl.org/syndication/thread/1.0");
            xnm.AddNamespace("usr", "http://streams.atlassian.com/syndication/username/1.0");
            xnm.AddNamespace("default", "http://www.w3.org/2005/Atom");
            XPathNodeIterator xpni = xpn.Select("//default:feed", xnm);

            while (xpni.MoveNext())
            {
                Console.WriteLine(xpni.Current.InnerXml);

            }
        }


        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance()
        {
            throw new NotImplementedException();
        }

        public string[] GetThreadStatus()
        {
            throw new NotImplementedException();
        }

        public event EventHandler ThreadFatalErrorEvent;
    }
}
