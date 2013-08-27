// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccStatus.cs" company="Helpmebot Development Team">
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
//   Defines the Accstatus type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



namespace helpmebot6.Commands
{
    using System;
    using System.Xml.XPath;

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
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
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
