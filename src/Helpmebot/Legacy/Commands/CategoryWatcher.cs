// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcher.cs" company="Helpmebot Development Team">
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
//   Category watcher command.
//   This is a class called explicitly from the command parser, as it's name us the category code,
//   so will behave slightly differently to other command classes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;
    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Category watcher command.
    /// <para>
    /// This is a class called explicitly from the command parser, as it's name us the category code,
    /// so will behave slightly differently to other command classes.
    /// </para>
    /// </summary>
    internal class CategoryWatcher : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CategoryWatcher"/> class.
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
        public CategoryWatcher(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            CommandResponseHandler crh = new CommandResponseHandler();
            if (this.Arguments.Length == 1)
            {
                var channel = this.CommandServiceHelper.ChannelRepository.GetByName(this.Channel);
                
                if (channel == null)
                {
                    return new CommandResponseHandler("This command must be run in-channel");
                }
                
                this.Log.DebugFormat("Triggering force-update of catwatcher for {0}", this.Arguments[0]);

                ServiceLocator.Current.GetInstance<ICategoryWatcherBackgroundService>()
                    .ForceUpdate(this.Arguments[0], channel);
            }
            
            return crh;
        }
    }
}