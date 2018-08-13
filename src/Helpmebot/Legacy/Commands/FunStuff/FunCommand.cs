// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunCommand.cs" company="Helpmebot Development Team">
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

namespace helpmebot6.Commands.FunStuff
{
    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;

    /// <summary>
    ///     The fun command.
    /// </summary>
    public abstract class FunCommand : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="FunCommand"/> class.
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
        protected FunCommand(
            LegacyUser source,
            string channel,
            string[] args,
            ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The on access denied.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected override CommandResponseHandler OnAccessDenied()
        {
            var channelRepository = this.CommandServiceHelper.ChannelRepository;
            var channel = channelRepository.GetByName(this.Channel);
            
            bool channelHedgehogMode = false;
            if (channel != null)
            {
                channelHedgehogMode = channel.HedgehogMode;
            }

            string message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                Messages.HedgehogAccessDenied,
                this.Channel,
                null);

            return channelHedgehogMode == false
                ? base.OnAccessDenied()
                : new CommandResponseHandler(message, CommandResponseDestination.PrivateMessage);
        }

        /// <summary>
        ///     The test access.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        protected override bool TestAccess()
        {
            var channelRepository = this.CommandServiceHelper.ChannelRepository;
            var channel = channelRepository.GetByName(this.Channel);

            if (channel == null)
            {
                return base.TestAccess();
            }

            return channel.HedgehogMode == false && base.TestAccess();
        }

        #endregion
    }
}