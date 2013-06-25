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

using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    /// Controls the newbie welcomer
    /// </summary>
    internal class Welcomer : GenericCommand
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
            var ignore = false;
            switch (args[0].ToLower())
            {
                case "enable":
                    if (Configuration.singleton()["welcomeNewbie",channel] == "true")
                    {
                        return new CommandResponseHandler(new Message().get("Newbie welcomer is already on"));
                    }
                    Configuration.singleton()["welcomeNewbie", channel]= "true";
                    return new CommandResponseHandler(new Message().get("The newbie welcomer has been turned on"));
                case "disable":
                    if (Configuration.singleton()["welcomeNewbie",channel] == "false")
                    {
                        return new CommandResponseHandler(new Message().get("Newbie welcomer is already off"));
                    }
                    Configuration.singleton()["welcomeNewbie", channel]= "false";
                    return new CommandResponseHandler(new Message().get("The newbie welcomer has been turned off"));
                case "global":
                    Configuration.singleton( )[ "welcomeNewbie", channel ] = null;
                    return new CommandResponseHandler(new Message().get("defaultSetting"));
                case "add":
                    ignore = true;
                    GlobalFunctions.popFromFront(ref args);
///Shouldn't there be a test to see if the name is already on the list?

                    NewbieWelcomer.instance().addHost(args[1], ignore);
                    return new CommandResponseHandler(new Message().get("done"));
                case "del":
                    ignore = true;
                    GlobalFunctions.popFromFront(ref args);
///Shouldn't there be a test to see if the name is actually on the list?

                    NewbieWelcomer.instance().delHost(args[1], ignore);
                    return new CommandResponseHandler(new Message().get("done"));
                case "list":
                    ignore = true;
                    GlobalFunctions.popFromFront(ref args);

                    var crh = new CommandResponseHandler();
                    string[] list = NewbieWelcomer.instance().getHosts(ignore);
                    foreach (string item in list)
                    {
                        crh.respond(item);
                    }
                    return crh;
            }
            return new CommandResponseHandler();
        }
    }
}
