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

using System;
using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands
{
    internal class CategoryWatcher : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponseHandler crh = new CommandResponseHandler();

            if (args.Length == 1)
            {
                // just do category check
                crh.respond(WatcherController.instance().forceUpdate(args[0], channel));
            }
            else
            {
                // do something else too.
                Type subCmdType =
                    Type.GetType("helpmebot6.Commands.CategoryWatcherCommand." + args[1].Substring(0, 1).ToUpper() +
                                 args[1].Substring(1).ToLower());
                if (subCmdType != null)
                {
                    return ((GenericCommand) Activator.CreateInstance(subCmdType)).run(source, channel, args);
                }
            }
            return crh;
        }
    }
}