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
namespace helpmebot6.Commands
{
    internal class Helper : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            // FIXME: this needs putting into its own subsystem, messageifying, configifying, etc.
            if (channel == "#wikipedia-en-help")
            {
                string message = "[HELP]: " + source + " needs help in #wikipedia-en-help!";
                if (args.Length > 0)
                    message += " (message: \"" + string.Join(" ", args) + "\")";

                Helpmebot6.irc.ircNotice("#wikipedia-en-helpers", message);
            }
            return null;
        }
    }
}