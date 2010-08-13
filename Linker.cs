#region Usings

using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

#endregion

namespace helpmebot6
{
    using System.Collections;
    using System.IO;
    using System.Text;

    public class Linker
    {
        private readonly Dictionary<string, string> _lastLink;

        private static Linker _singleton;

        protected Linker()
        {
            this._lastLink = new Dictionary<string, string>();
            Helpmebot6.irc.privmsgEvent += irc_PrivmsgEvent;
            Helpmebot6.irc.noticeEvent += irc_PrivmsgEvent;
        }

        private void irc_PrivmsgEvent(User source, string destination, string message)
        {
            this.parseMessage(message, destination);
        }

        public static Linker instance()
        {
            return _singleton ?? ( _singleton = new Linker( ) );
        }

        public void parseMessage(string message, string channel)
        {
            ArrayList newLink = reallyParseMessage(message);
            if (newLink.Count == 0)
                return;
            if (this._lastLink.ContainsKey(channel))
            {
                this._lastLink.Remove(channel);
            }
            this._lastLink.Add(channel, (string)newLink[0]);
            this.sendLink(channel, (string)newLink[0]);
        }

        public ArrayList reallyParseMessage(string message)
        {
            ArrayList newLinks = new ArrayList();

            Regex linkRegex = new Regex(@"\[\[([^\[\]]*)\]\]|{{([^{}]*)}}");
            Match m = linkRegex.Match( message );
            while (m.Length > 0)
            {
                if ( m.Groups[ 1 ].Length > 0 )
                    newLinks.Add( m.Groups[ 1 ].Value );
                if ( m.Groups[ 2 ].Length > 0 )
                    newLinks.Add( "Template:" + m.Groups[ 2 ].Value );

                m = m.NextMatch( );
            }

            return newLinks;
        }

        public string getLink(string destination)
        {
            return this.getLink(destination, false);
        }

        public string getLink(string destination, bool useSecureServer)
        {
            string link;
            bool success = this._lastLink.TryGetValue(destination, out link);
            return success ? getRealLink( destination, link, useSecureServer ) : "";
        }

        public static string getRealLink( string destination, string link, bool useSecureServer )
        {
            string iwprefix = link.Split(':')[0];

            
           string url = DAL.singleton().proc_HMB_GET_IW_URL(iwprefix);


            if (link.Split(':').Length == 1 || url == string.Empty)
            {
                url =
                    Configuration.singleton( ).retrieveLocalStringOption(
                        ( useSecureServer ? "wikiSecureUrl" : "wikiUrl" ), destination );
                return url + antispace( link );
            }
            return url.Replace("$1", antispace(string.Join(":", link.Split(':'), 1, link.Split(':').Length - 1)));
        }

        private static string antispace(string source)
        {
            int currloc = 0;
            string result = "";
            while (currloc < source.Length)
            {
                if (source.Substring(currloc, 1) == " ")
                {
                    result += "_";
                }
                else
                {
                    result += source.Substring(currloc, 1);
                }
                currloc += 1;
            }
            return result;
        }

        private void sendLink(string channel, string link)
        {
            if (Configuration.singleton().retrieveLocalStringOption("autoLink", channel) == "true")
                Helpmebot6.irc.ircPrivmsg(channel, this.getLink(link, false));
        }
    }
}