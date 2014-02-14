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
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using System.Globalization;

    using Castle.Core.Logging;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using MySql.Data.MySqlClient;

    /// <summary>
    ///     Modifies the bot's access list
    /// </summary>
    internal class Access : GenericCommand
    {
        #region Constructors and Destructors

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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Access(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var crh = new CommandResponseHandler();
            IMessageService messageService = this.CommandServiceHelper.MessageService;
            if (this.Arguments.Length > 1)
            {
                switch (this.Arguments[0].ToLower())
                {
                    case "add":
                        if (this.Arguments.Length > 2)
                        {
                            var aL = LegacyUser.UserRights.Normal;

                            switch (this.Arguments[2].ToLower())
                            {
                                case "developer":
                                    aL = this.Source.AccessLevel == LegacyUser.UserRights.Developer
                                             ? LegacyUser.UserRights.Developer
                                             : LegacyUser.UserRights.Superuser;
                                    break;
                                case "superuser":
                                    aL = LegacyUser.UserRights.Superuser;
                                    break;
                                case "advanced":
                                    aL = LegacyUser.UserRights.Advanced;
                                    break;
                                case "semi-ignored":
                                    aL = LegacyUser.UserRights.Semiignored;
                                    break;
                                case "semiignored":
                                    aL = LegacyUser.UserRights.Semiignored;
                                    break;
                                case "ignored":
                                    aL = LegacyUser.UserRights.Ignored;
                                    break;
                                case "normal":
                                    aL = LegacyUser.UserRights.Normal;
                                    break;
                            }

                            crh = this.AddAccessEntry(LegacyUser.NewFromString(this.Arguments[1]), aL);
                        }
                        else
                        {
                            string[] messageParameters =
                                {
                                    "access add", "3", 
                                    this.Arguments.Length.ToString(CultureInfo.InvariantCulture)
                                };
                            return
                                new CommandResponseHandler(
                                    messageService.RetrieveMessage(
                                        "notEnoughParameters", 
                                        this.Channel, 
                                        messageParameters));
                        }

                        break;
                    case "del":
                        crh = this.DeleteAccessEntry(int.Parse(this.Arguments[1]));
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
                string[] messageParameters =
                    {
                        "access", "2", 
                        this.Arguments.Length.ToString(CultureInfo.InstalledUICulture)
                    };
                return
                    new CommandResponseHandler(
                        messageService.RetrieveMessage("notEnoughParameters", this.Channel, messageParameters));
            }

            return crh;
        }

        /// <summary>
        ///     Access denied to command, decide what to do
        /// </summary>
        /// <returns>
        ///     A response to the command if access to the command was denied
        /// </returns>
        protected override CommandResponseHandler OnAccessDenied()
        {
            CommandResponseHandler crh =
                new Myaccess(this.Source, this.Channel, this.Arguments, this.CommandServiceHelper)
                    .RunCommand();
            return crh;
        }

        /// <summary>
        /// Adds the access entry.
        /// </summary>
        /// <param name="newEntry">
        /// The new entry.
        /// </param>
        /// <param name="accessLevel">
        /// The access level.
        /// </param>
        /// <returns>
        /// a response
        /// </returns>
        private CommandResponseHandler AddAccessEntry(LegacyUser newEntry, LegacyUser.UserRights accessLevel)
        {
            string[] messageParams = { newEntry.ToString(), accessLevel.ToString() };
            string message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                "addAccessEntry", 
                this.Channel, 
                messageParams);

            // "Adding access entry for " + newEntry.ToString( ) + " at level " + AccessLevel.ToString( )"
            ServiceLocator.Current.GetInstance<ILogger>()
                .Info(string.Format("Adding access entry for {0} at level {1}", newEntry, accessLevel));

            var command = new MySqlCommand("INSERT INTO user VALUES ( null, @nick, @user, @host, @accesslevel, null );");

            command.Parameters.AddWithValue("@nick", newEntry.Nickname);
            command.Parameters.AddWithValue("@user", newEntry.Username);
            command.Parameters.AddWithValue("@host", newEntry.Hostname);
            command.Parameters.AddWithValue("@accesslevel", accessLevel.ToString());

            LegacyDatabase.Singleton().ExecuteCommand(command);

            return new CommandResponseHandler(message);
        }

        /// <summary>
        /// Deletes the access entry.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// a response
        /// </returns>
        private CommandResponseHandler DeleteAccessEntry(int id)
        {
            string[] messageParams = { id.ToString(CultureInfo.InvariantCulture) };
            string message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                "removeAccessEntry", 
                this.Channel, 
                messageParams);

            ServiceLocator.Current.GetInstance<ILogger>().Info(string.Format("Removing access entry #{0}", id));

            var deleteCommand = new MySqlCommand("DELETE FROM user WHERE user_id = @userid LIMIT 1;");
            deleteCommand.Parameters.AddWithValue("@userid", id);
            LegacyDatabase.Singleton().ExecuteCommand(deleteCommand);

            return new CommandResponseHandler(message);
        }

        #endregion
    }
}