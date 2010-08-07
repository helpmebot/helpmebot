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

using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands
{
    internal class Welcomer : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            switch (args[0].ToLower())
            {
                case "enable":
                    if (Configuration.singleton().retrieveLocalStringOption("welcomeNewbie", channel) == "true")
                    {
                        return new CommandResponseHandler(Configuration.singleton().getMessage("no-change"));
                    }
                    Configuration.singleton().oldSetLocalOption("welcomeNewbie", channel, "true");
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "disable":
                    if (Configuration.singleton().retrieveLocalStringOption("welcomeNewbie", channel) == "false")
                    {
                        return new CommandResponseHandler(Configuration.singleton().getMessage("no-change"));
                    }
                    Configuration.singleton().oldSetLocalOption("welcomeNewbie", channel, "false");
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "global":
                    Configuration.singleton().deleteLocalOption("welcomeNewbie", channel);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("defaultSetting"));
                case "addOrder":
                    NewbieWelcomer.instance().addHost(args[1]);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "del":
                    NewbieWelcomer.instance().delHost(args[1]);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "list":
                    CommandResponseHandler crh = new CommandResponseHandler();
                    string[] list = NewbieWelcomer.instance().getHosts();
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