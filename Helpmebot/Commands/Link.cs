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
    using System.Collections;
    using System.Linq;

    using Helpmebot;
    using Helpmebot.ExtensionMethods;

    /// <summary>
    /// Triggers the link parser
    /// </summary>
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
        public Link(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The result</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            bool secure = bool.Parse(Configuration.singleton()["useSecureWikiServer", this.Channel]);
            string[] args = this.Arguments;
            if (args.Length > 0)
            {
                if (args[0] == "@secure")
                {
                    secure = true;
                    GlobalFunctions.popFromFront(ref args);
                }
            }

            if (args.SmartLength() > 0)
            {
                ArrayList links = Linker.instance().reallyParseMessage(string.Join(" ", args));

                if (links.Count == 0)
                {
                    links = Linker.instance().reallyParseMessage("[[" + string.Join(" ", args) + "]]");
                }

                string message = links.Cast<string>()
                    .Aggregate(string.Empty, (current, link) => current + " " + Linker.getRealLink(this.Channel, link, secure));

                return new CommandResponseHandler(message);
            }

            return new CommandResponseHandler(Linker.instance().getLink(this.Channel, secure));
        }
    }
}