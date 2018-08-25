// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Link.cs" company="Helpmebot Development Team">
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
//   Triggers the link parser
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Linq;

    using Helpmebot;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Services;
    using Helpmebot.Services.Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// Triggers the link parser
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Semiignored)]
    internal class Link : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Link"/> class.
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
        public Link(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The result</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            // fixme: servicelocator
            var linker = ServiceLocator.Current.GetInstance<ILinkerService>();
            
            var args = this.Arguments;

            if (args.SmartLength() > 0)
            {
                var links = linker.ParseMessageForLinks(string.Join(" ", args));

                if (links.Count == 0)
                {
                    links = linker.ParseMessageForLinks("[[" + string.Join(" ", args) + "]]");
                }

                var message = links.Aggregate(
                    string.Empty,
                    (current, link) => current + " " + linker.ConvertWikilinkToUrl(this.Channel, link));
                return new CommandResponseHandler(message.Trim());
            }

            return new CommandResponseHandler(linker.GetLastLinkForChannel(this.Channel));
        }
    }
}