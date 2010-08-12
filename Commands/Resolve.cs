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

using System.Net;
using System.Net.Sockets;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    /// Perform a reverse DNS lookup on an IP address.
    /// </summary>
    internal class Resolve : GenericCommand
    {
        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            IPAddress[] addresses = new IPAddress[0];
            try
            {
                addresses = Dns.GetHostEntry(args[0]).AddressList;
            }
            catch (SocketException)
            {
            }
            if (addresses.Length != 0)
            {
                string ipList = "";
                bool first = true;
                foreach (IPAddress item in addresses)
                {
                    if (!first)
                        ipList += ", ";
                    ipList += item.ToString();
                    first = false;
                }
                string[] messageargs = {args[0], ipList};

                return new CommandResponseHandler(new Message().get("resolve", messageargs));
            }
            else
            {
                string[] messageargs = {args[0]};
                return new CommandResponseHandler(new Message().get("resolveFail", messageargs));
            }
        }
    }
}