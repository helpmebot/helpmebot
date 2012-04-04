using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.XPath;

namespace helpmebot6.Commands
{
    class AccCount : GenericCommand
    {
        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string username;

            if (args.Length > 0 && args[0] != "")
            {
                username = string.Join(" ", args);
            }
            else
            {
                username = source.nickname;
            }

            username = HttpUtility.UrlEncode(username);

            XPathDocument xpd =
                new XPathDocument(
                    HttpRequest.get("http://toolserver.org/~acc/api.php?action=count&user=" + username));

            XPathNodeIterator xpni = xpd.CreateNavigator().Select("//user");


            if (xpni.MoveNext())
            {
                if(xpni.Current.GetAttribute("missing", "") == "true")
                {
                    string[] msgparams = {username};
                    string msg = new Message().get("noSuchUser", msgparams);
                    return new CommandResponseHandler(msg);
                }

                string[] adminparams = {
                                            xpni.Current.GetAttribute("suspended", ""), 
                                            xpni.Current.GetAttribute("promoted", ""), 
                                            xpni.Current.GetAttribute("approved", ""), 
                                            xpni.Current.GetAttribute("demoted", ""), 
                                            xpni.Current.GetAttribute("declined", ""), 
                                            xpni.Current.GetAttribute("renamed", ""), 
                                            xpni.Current.GetAttribute("edited", ""), 
                                            xpni.Current.GetAttribute("prefchange", ""), 
                                         };

                string adminmessage = new Message().get("CmdAccCountAdmin", adminparams);

                string[] messageParams = {
                                            username, // username
                                            xpni.Current.GetAttribute("level", ""), // accesslevel
                                            xpni.Current.GetAttribute("created", ""), // numclosed
                                            xpni.Current.GetAttribute("today", ""), // today
                                            xpni.Current.GetAttribute("level", "") == "Admin" ? adminmessage : ""// admin
                                         };

                string message = new Message().get("CmdAccCount", messageParams);
                return new CommandResponseHandler(message);

            }

            throw new ArgumentException();
        }

        #endregion
    }
}
