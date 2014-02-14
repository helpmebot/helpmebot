// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Autolink.cs" company="Helpmebot Development Team">
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
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///     Enables or disables automatic parsing of wiki links
    /// </summary>
    internal class Autolink : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Autolink"/> class.
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
        public Autolink(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
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
            bool global = false;

            string[] args = this.Arguments;

            if (args.Length > 0)
            {
                if (args[0].ToLower() == "@global")
                {
                    global = true;
                    GlobalFunctions.PopFromFront(ref args);
                }
            }

            bool oldValue =
                bool.Parse(
                    !global ? LegacyConfig.Singleton()["autoLink", this.Channel] : LegacyConfig.Singleton()["autoLink"]);

            IMessageService messageService = this.CommandServiceHelper.MessageService;
            if (args.Length > 0)
            {
                string newValue = "global";
                switch (args[0].ToLower())
                {
                    case "enable":
                        newValue = "true";
                        break;
                    case "disable":
                        newValue = "false";
                        break;
                    case "global":
                        newValue = "global";
                        break;
                }

                if (newValue == oldValue.ToString().ToLower())
                {
                    return
                        new CommandResponseHandler(
                            messageService.RetrieveMessage(Messages.NoChange, this.Channel, null), 
                            CommandResponseDestination.PrivateMessage);
                }

                if (newValue == "global")
                {
                    LegacyConfig.Singleton()["autoLink", this.Channel] = null;
                    return
                        new CommandResponseHandler(
                            messageService.RetrieveMessage(Messages.DefaultConfig, this.Channel, null), 
                            CommandResponseDestination.PrivateMessage);
                }

                if (!global)
                {
                    LegacyConfig.Singleton()["autoLink", this.Channel] = newValue;
                }
                else
                {
                    LegacyConfig.Singleton()["autoLink"] = newValue;
                }

                return new CommandResponseHandler(
                    messageService.RetrieveMessage(Messages.Done, this.Channel, null), 
                    CommandResponseDestination.PrivateMessage);
            }

            string[] mP = { "autolink", 1.ToString(), args.Length.ToString() };

            return
                new CommandResponseHandler(
                    messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, mP), 
                    CommandResponseDestination.PrivateMessage);
        }

        #endregion
    }
}