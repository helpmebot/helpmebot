using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using helpmebot6.Threading;

namespace helpmebot6.Monitoring.RSS
{
    class RssMonitor : IThreadedSystem
    {
        public RssMonitor()
        {
                
        }

      //private Thread _t = new Thread(new ThreadStart(execute));

        string feedurl = "https://jira.toolserver.org/plugins/servlet/streams?key=ACC";

        void execute()
        {
            

            HttpWebRequest wreq = WebRequest.Create(feedurl) as HttpWebRequest;
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


        public void stop()
        {
            throw new NotImplementedException();
        }

        public void registerInstance()
        {
            throw new NotImplementedException();
        }

        public string[] getThreadStatus()
        {
            throw new NotImplementedException();
        }

        public event EventHandler threadFatalError;
    }
}
