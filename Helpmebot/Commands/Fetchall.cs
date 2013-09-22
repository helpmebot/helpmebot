// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Fetchall.cs" company="Helpmebot Development Team">
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
//   Retrieve information about all registered category codes
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    using helpmebot6.Monitoring;

    /// <summary>
    /// Retrieve information about all registered category codes
    /// </summary>
    internal class Fetchall : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Fetchall"/> class.
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
        public Fetchall(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            CommandResponseHandler crh = new CommandResponseHandler();
            Dictionary<string, Monitoring.CategoryWatcher>.KeyCollection kc = WatcherController.instance().getKeywords();
            string[] args = this.Arguments;
            if (args.Contains("@cats"))
            {
                GlobalFunctions.removeItemFromArray("@cats", ref args);
                string listSep = new Message().get("listSeparator");
                string list = new Message().get("allCategoryCodes");
                foreach (string item in kc)
                {
                    list += item;
                    list += listSep;
                }

                crh.respond(list.TrimEnd(listSep.ToCharArray()));
            }
            else
            {
                foreach (string key in kc)
                {
                    crh.respond(WatcherController.instance().forceUpdate(key, this.Channel), CommandResponseDestination.PrivateMessage);
                }
            }
            
            return crh;
        }
    }
}