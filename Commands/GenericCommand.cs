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

#endregion

namespace helpmebot6.Commands
{
    internal abstract class GenericCommand
    {
        /// <summary>
        ///   Access level of the command
        /// </summary>
        public User.UserRights accessLevel
        {
            get
            {
                string command = GetType().ToString();

                DAL.Select q = new DAL.Select("accesslevel");
                q.setFrom("command");
                q.addLimit(1, 0);
                q.addWhere(new DAL.WhereConds("typename", command));

                string al = DAL.singleton().executeScalarSelect(q);
                try
                {
                    return (User.UserRights) Enum.Parse(typeof (User.UserRights), al, true);
                }
                catch (ArgumentException)
                {
                    Logger.instance().addToLog("Warning: " + command + " not found in access list.",
                                               Logger.LogTypes.Error);
                    return User.UserRights.Developer;
                }
            }
        }

        /// <summary>
        ///   Trigger an exectution of the command
        /// </summary>
        /// <param name = "source"></param>
        /// <param name = "channel"></param>
        /// <param name = "args"></param>
        /// <returns></returns>
        public CommandResponseHandler run(User source, string channel, string[] args)
        {
            string command = GetType().ToString();

            this.log("Running command: " + command);

            return accessTest(source, channel)? this.reallyRun(source,channel,args  ):this.accessDenied(source,command,args  );
        }

        /// <summary>
        ///   Check the access level and then decide what to do.
        /// </summary>
        /// <param name = "source"></param>
        /// <param name = "channel"></param>
        /// <returns></returns>
        protected virtual bool accessTest(User source, string channel)
        {
            // check the access level
            return source.accessLevel >= this.accessLevel ? true : false;
        }

        /// <summary>
        ///   Access granted to command, decide what to do
        /// </summary>
        /// <param name = "source"></param>
        /// <param name = "channel"></param>
        /// <param name = "args"></param>
        /// <returns></returns>
        protected virtual CommandResponseHandler reallyRun(User source, string channel, string[] args)
        {
            AccessLog.instance().save(new AccessLog.AccessLogEntry(source, GetType(), true));
            this.log("Starting command execution...");
            CommandResponseHandler crh;
            try
            {
                crh = execute(source, channel, args);
            }
            catch (Exception ex)
            {
                Logger.instance().addToLog(ex.ToString(), Logger.LogTypes.Error);
                crh = new CommandResponseHandler(ex.Message);
            }
            this.log("Command execution complete.");
            return crh;
        }

        /// <summary>
        ///   Access denied to command, decide what to do
        /// </summary>
        /// <param name = "source"></param>
        /// <param name = "channel"></param>
        /// <param name = "args"></param>
        /// <returns></returns>
        protected virtual CommandResponseHandler accessDenied(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponseHandler response = new CommandResponseHandler();

            response.respond(Configuration.singleton().getMessage("accessDenied", ""),
                             CommandResponseDestination.PrivateMessage);
            this.log("Access denied to command.");
            AccessLog.instance().save(new AccessLog.AccessLogEntry(source, GetType(), false));
            return response;
        }

        /// <summary>
        ///   Actual command logic
        /// </summary>
        /// <param name = "source"></param>
        /// <param name = "channel"></param>
        /// <param name = "args"></param>
        /// <returns></returns>
        protected abstract CommandResponseHandler execute(User source, string channel, string[] args);

        protected void log(string message)
        {
            Logger.instance( ).addToLog( MethodBase.GetCurrentMethod( ).DeclaringType.Name + ": " + message,
                                         Logger.LogTypes.Command );
        }
    }
}