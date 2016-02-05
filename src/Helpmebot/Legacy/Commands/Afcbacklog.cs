// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Afcbacklog.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Returns the number of articles currently waiting at Articles for Creation
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;

    /// <summary>
    /// Returns the number of articles currently waiting at Articles for Creation
    /// </summary>
    internal class Afcbacklog : Categorysize
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Afccount"/> class.
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
        public Afcbacklog(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string baseWiki = LegacyConfig.Singleton()["baseWiki", this.Channel];

            MediaWikiSite mediaWikiSite = this.CommandServiceHelper.MediaWikiSiteRepository.GetById(int.Parse(baseWiki));
            var size = mediaWikiSite.GetCategorySize("Pending AfC submissions");

            if (size == 0)
            {
                // TODO: push these to the wiki?
                return new CommandResponseHandler("There are no new AfC submissions.");
            }
            else if (size < 200)
            {
                return new CommandResponseHandler("AfC is clearing out.");
            }
            else if (size < 400)
            {
                return new CommandResponseHandler("There is a normal backlog at AfC.");
            }
            else if (size < 650)
            {
                return new CommandResponseHandler("AfC is semi-backlogged at the moment.");
            }
            else if (size < 900)
            {
                return new CommandResponseHandler("There is a backlog at AfC.");
            }
            else if (size < 1200)
            {
                return new CommandResponseHandler("AfC is highly backlogged at the moment.");
            }
            else if (size < 2000)
            {
                return new CommandResponseHandler("There is a severe backlog at AfC.");
            }
            else if (size < 4000)
            {
                return new CommandResponseHandler("AfC is critically backlogged.");
            }
            else if (size < 10000)
            {
                return new CommandResponseHandler("AfC is out of order.");
            }
            else
            {
                return new CommandResponseHandler(
                    "Unknown error occurred, current status of AfC is unknown.", 
                    CommandResponseDestination.PrivateMessage);
            }
        }
    }
}