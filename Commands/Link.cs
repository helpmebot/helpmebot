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

#endregion

namespace helpmebot6.Commands
{
    using System.Collections;
    using System.Linq;

    internal class Link : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                string.Format("Method:{0}{1}", MethodBase.GetCurrentMethod().DeclaringType.Name,
                              MethodBase.GetCurrentMethod().Name), Logger.LogTypes.DNWB);

            bool secure = bool.Parse(Configuration.singleton().retrieveLocalStringOption("useSecureWikiServer", channel));
            if (args.Length > 0)
            {
                if (args[0] == "@secure")
                {
                    secure = true;
                    GlobalFunctions.popFromFront(ref args);
                }
            }

            if (GlobalFunctions.realArrayLength(args) > 0)
            {
               ArrayList links = Linker.instance().reallyParseMessage(string.Join(" ", args));

                string message = links.Cast<string>( ).Aggregate( "", ( current, link ) => current + " "+ Linker.getRealLink( channel, link, secure ) );

                return new CommandResponseHandler(message);
            }
            return new CommandResponseHandler( Linker.instance( ).getLink( channel, secure ) );
        }
    }
}