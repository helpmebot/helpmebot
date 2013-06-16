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

using System.Collections.Generic;
using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    /// Retrieve information about all registered category codes
    /// </summary>
    internal class Fetchall : GenericCommand
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
            CommandResponseHandler crh = new CommandResponseHandler();
            Dictionary<string, Monitoring.CategoryWatcher>.KeyCollection kc = WatcherController.instance().getKeywords();
            if (GlobalFunctions.isInArray("@cats", args) != -1)
            {
                GlobalFunctions.removeItemFromArray("@cats", ref args);
                string listSep = new Message().get("listSeparator");
                string list = new Message().get("allCategoryCodes");
                foreach (string item in kc)
                {
                    list += item;
                    list += listSep;
                }

                crh.respond(list.TrimEnd(listSep.ToCharArray()));
            }
            else
            {
                foreach (string key in kc)
                {
                    crh.respond(WatcherController.instance().forceUpdate(key, channel), CommandResponseDestination.PrivateMessage);
                }
            }
            
            return crh;
        }
    }
}