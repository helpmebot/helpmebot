// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Access.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Modifies the bot's access list
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Modifies the bot's access list
    /// </summary>
    internal class Access : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Access"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public Access(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Access denied to command, decide what to do
        /// </summary>
        /// <returns>
        /// A response to the command if access to the command was denied
        /// </returns>
        protected override CommandResponseHandler OnAccessDenied()
        {
            CommandResponseHandler crh = new Myaccess(this.Source, this.Channel, this.Arguments).RunCommand();
            return crh;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var crh = new CommandResponseHandler();
            if (this.Arguments.Length > 1)
            {
                switch (this.Arguments[0].ToLower())
                {
                    case "add":
                        if (this.Arguments.Length > 2)
                        {
                            var aL = User.UserRights.Normal;

                            switch (this.Arguments[2].ToLower())
                            {
                                case "developer":
                                    aL = this.Source.accessLevel == User.UserRights.Developer
                                             ? User.UserRights.Developer
                                             : User.UserRights.Superuser;
                                    break;
                                case "superuser":
                                    aL = User.UserRights.Superuser;
                                    break;
                                case "advanced":
                                    aL = User.UserRights.Advanced;
                                    break;
                                case "semi-ignored":
                                    aL = User.UserRights.Semiignored;
                                    break;
                                case "semiignored":
                                    aL = User.UserRights.Semiignored;
                                    break;
                                case "ignored":
                                    aL = User.UserRights.Ignored;
                                    break;
                                case "normal":
                                    aL = User.UserRights.Normal;
                                    break;
                            }

                            crh = AddAccessEntry(User.newFromString(this.Arguments[1]), aL);
                        }
                        else
                        {
                            string[] messageParameters = { "access add", "3", this.Arguments.Length.ToString() };
                            return new CommandResponseHandler(new Message().get("notEnoughParameters", messageParameters));

                        }

                        break;
                    case "del":
                        crh = DeleteAccessEntry(int.Parse(this.Arguments[1]));
                        break;
                }

                /*
                 * add <source> <level>
                 *
                 * del <id>
                 */
            }
            else
            {
                string[] messageParameters = { "access", "2", this.Arguments.Length.ToString() };
                return new CommandResponseHandler(new Message().get("notEnoughParameters", messageParameters));
            }

            return crh;
        }

        /// <summary>
        /// Adds the access entry.
        /// </summary>
        /// <param name="newEntry">The new entry.</param>
        /// <param name="accessLevel">The access level.</param>
        /// <returns>a response</returns>
        [Obsolete("Use User class")]
        private static CommandResponseHandler AddAccessEntry(User newEntry, User.UserRights accessLevel)
        {
            Logger.instance()
                .addToLog(
                    "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                    Logger.LogTypes.DNWB);

            string[] messageParams = {newEntry.ToString(), accessLevel.ToString()};
            string message = new Message().get("addAccessEntry", messageParams);

            // "Adding access entry for " + newEntry.ToString( ) + " at level " + AccessLevel.ToString( )"
            Logger.instance()
                .addToLog("Adding access entry for " + newEntry + " at level " + accessLevel, Logger.LogTypes.Command);
            DAL.singleton()
                .insert(
                    "user",
                    string.Empty,
                    newEntry.nickname,
                    newEntry.username,
                    newEntry.hostname,
                    accessLevel.ToString(),
                    string.Empty);

            return new CommandResponseHandler(message);
        }

        /// <summary>
        /// Dels the access entry.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>a response</returns>
        [Obsolete("Use the User class")]
        private static CommandResponseHandler DeleteAccessEntry(int id)
        {
            Logger.instance()
                .addToLog(
                    "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                    Logger.LogTypes.DNWB);

            string[] messageParams = { id.ToString() };
            string message = new Message().get("removeAccessEntry", messageParams);

            Logger.instance().addToLog("Removing access entry #" + id, Logger.LogTypes.Command);
            DAL.singleton().delete("user", 1, new DAL.WhereConds("user_id", id.ToString()));

            return new CommandResponseHandler(message);
        }
    }
}