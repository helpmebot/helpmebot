// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Blockuser.cs" company="Helpmebot Development Team">
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
//   Retrieves a link to block a user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Services;
    using Helpmebot.Services.Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// Retrieves a link to block a user.
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Blockuser : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Blockuser"/> class.
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
        public Blockuser(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] args = this.Arguments;
            string name = string.Join(" ", args);
            string prefix = string.Empty;
            string page = "Special:Block/";

            if (name.Contains(":"))
            {
                string origname = name;

                string[] parts = name.Split(new[] { ':' }, 2);
                name = parts[1];
                prefix = parts[0];

                if (this.CommandServiceHelper.InterwikiPrefixRepository.GetByPrefix(prefix) == null)
                {
                    name = origname;
                    prefix = string.Empty;
                }
                else
                {
                    prefix += ":";
                }
            }

            // fixme: servicelocator
            var linker = ServiceLocator.Current.GetInstance<ILinkerService>();
            
            string url = linker.ConvertWikilinkToUrl(this.Channel, prefix + page + name).Replace("\0", string.Empty);

            return new CommandResponseHandler(url);
        }
    }
}