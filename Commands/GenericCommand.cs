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

            return accessTest(source, channel, args);
        }

        /// <summary>
        ///   Check the access level and then decide what to do.
        /// </summary>
        /// <param name = "source"></param>
        /// <param name = "channel"></param>
        /// <param name = "args"></param>
        /// <returns></returns>
        protected virtual CommandResponseHandler accessTest(User source, string channel, string[] args)
        {
            // check the access level
            if ( source.accessLevel >= this.accessLevel )
                return this.reallyRun( source, channel, args );
            return this.accessDenied(source, channel, args);
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
            Logger.instance().addToLog(message, Logger.LogTypes.Command);
        }
    }
}