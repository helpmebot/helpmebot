// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Myaccess.cs" company="Helpmebot Development Team">
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
    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    ///     Retrieves the bot access level of the user who called the command
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Semiignored)]
    internal class Myaccess : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Myaccess"/> class.
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
        public Myaccess(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
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
            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                foreach (string s in this.Arguments)
                {
                    var constructedUser = IrcUser.FromPrefix(s, this.CommandServiceHelper.Client);

                    if (constructedUser == null)
                    {
                        string[] errArgs = { s };
                        crh.Respond(messageService.RetrieveMessage("cmdAccessInvalidUser", this.Channel, errArgs));
                        continue;
                    }

                    if (!s.Contains("@") || !s.Contains("!"))
                    {
                        if (this.CommandServiceHelper.Client.UserCache.ContainsKey(constructedUser.Nickname))
                        {
                            var ircUser = this.CommandServiceHelper.Client.UserCache[constructedUser.Nickname];

                            constructedUser = ircUser;
                        }
                    }


                    string[] cmdArgs =
                    {
                        constructedUser.ToString(),
                        this.CommandServiceHelper.LegacyAccessService.GetLegacyUserRights(constructedUser).ToString()
                    };
                    crh.Respond(messageService.RetrieveMessage("cmdAccess", this.Channel, cmdArgs));
                }
            }
            else
            {
                string[] cmdArgs =
                {
                    this.Source.ToString(),
                    this.CommandServiceHelper.LegacyAccessService.GetLegacyUserRights(this.Source).ToString()
                };
                crh.Respond(messageService.RetrieveMessage("cmdAccess", this.Channel, cmdArgs));
            }

            return crh;
        }

        #endregion
    }
}