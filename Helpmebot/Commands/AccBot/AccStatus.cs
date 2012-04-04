using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.XPath;

namespace helpmebot6.Commands
{
    class Accstatus : GenericCommand
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

            XPathDocument xpd =
                new XPathDocument(
                    HttpRequest.get("http://toolserver.org/~acc/api.php?action=status"));

            XPathNodeIterator xpni = xpd.CreateNavigator().Select("//status");


            if (xpni.MoveNext())
            {
                string[] messageParams = {
                                            xpni.Current.GetAttribute("open", ""), 
                                            xpni.Current.GetAttribute("admin", ""), 
                                            xpni.Current.GetAttribute("checkuser", ""), 
                                            xpni.Current.GetAttribute("bans", ""), 
                                            xpni.Current.GetAttribute("useradmin", ""), 
                                            xpni.Current.GetAttribute("user", ""), 
                                            xpni.Current.GetAttribute("usernew", ""), 
                                         };

                string message = new Message().get("CmdAccStatus", messageParams);
                return new CommandResponseHandler(message);

            }

            throw new ArgumentException();
        }

        #endregion
    }
}
