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

using System.Collections;
using System.Reflection;

#endregion

namespace helpmebot6
{
    internal enum CommandResponseDestination
    {
        Default,
        ChannelDebug,
        PrivateMessage
    }

    internal struct CommandResponse
    {
        public CommandResponseDestination destination;
        public string message;
    }

    internal class CommandResponseHandler
    {
        private readonly ArrayList _responses;

        public CommandResponseHandler()
        {
            this._responses = new ArrayList();
        }

        public CommandResponseHandler(string message)
        {
            this._responses = new ArrayList();
            respond(message);
        }

        public CommandResponseHandler(string message, CommandResponseDestination destination)
        {
            this._responses = new ArrayList();
            respond(message, destination);
        }

        public void respond(string message)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponse cr;
            cr.destination = CommandResponseDestination.Default;
            cr.message = message;

            this._responses.Add(cr);
        }

        public void respond(string message, CommandResponseDestination destination)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponse cr;
            cr.destination = destination;
            cr.message = message;

            this._responses.Add(cr);
        }

        public void append(CommandResponseHandler moreResponses)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            foreach (object item in moreResponses.getResponses())
            {
                this._responses.Add(item);
            }
        }

        public ArrayList getResponses()
        {
            return this._responses;
        }
    }
}