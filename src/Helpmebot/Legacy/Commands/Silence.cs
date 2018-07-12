// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Silence.cs" company="Helpmebot Development Team">
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
    using Helpmebot.Model;

    /// <summary>
    ///     Controls the bots silencer
    /// </summary>
    internal class Silence : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Silence"/> class.
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
        public Silence(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
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
            string[] args = this.Arguments;

            var channelRepository = this.CommandServiceHelper.ChannelRepository;
            var channel = channelRepository.GetByName(this.Channel);
            var oldValue = channel.Silenced;

            var messageService = this.CommandServiceHelper.MessageService;

            if (args.Length > 0)
            {
                bool? newValue = null;
                switch (args[0].ToLower())
                {
                    case "enable":
                        newValue = true;
                        break;
                    case "disable":
                        newValue = false;
                        break;
                }

                if (newValue == oldValue || !newValue.HasValue)
                {
                    return
                        new CommandResponseHandler(
                            messageService.RetrieveMessage(Messages.NoChange, this.Channel, null), 
                            CommandResponseDestination.PrivateMessage);
                }

                channel.Silenced = newValue.Value;
                channelRepository.Save(channel);

                return new CommandResponseHandler(
                    messageService.RetrieveMessage(Messages.Done, this.Channel, null), 
                    CommandResponseDestination.PrivateMessage);
            }

            string[] mP = { "silence", 1.ToString(), args.Length.ToString() };
            return
                new CommandResponseHandler(
                    messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, mP), 
                    CommandResponseDestination.PrivateMessage);
        }

        #endregion
    }
}