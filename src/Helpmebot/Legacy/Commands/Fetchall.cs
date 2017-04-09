﻿// --------------------------------------------------------------------------------------------------------------------
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

using System.Net;

namespace helpmebot6.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Monitoring;

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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Fetchall(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var crh = new CommandResponseHandler();
            Dictionary<string, Helpmebot.Monitoring.CategoryWatcher>.KeyCollection kc = WatcherController.Instance().GetKeywords();

            List<string> args = this.Arguments.ToList();

            if (args.Contains("@cats"))
            {
                args.Remove("@cats");

                var messageService = this.CommandServiceHelper.MessageService;
                string listSep = messageService.RetrieveMessage("listSeparator", this.Channel, null);
                string list = messageService.RetrieveMessage("allCategoryCodes", this.Channel, null);

                foreach (string item in kc)
                {
                    list += item;
                    list += listSep;
                }

                crh.Respond(list.TrimEnd(listSep.ToCharArray()));
            }
            else
            {
                foreach (string key in kc)
                {
                    try
                    {
                        crh.Respond(WatcherController.Instance().ForceUpdate(key, this.Channel),
                            CommandResponseDestination.PrivateMessage);
                    }
                    catch (WebException ex)
                    {
                        crh.Respond(string.Format("Unable to contact Wikipedia API while processing key {0}: {1}", key, ex.Message));
                    }
                }
            }
            
            return crh;
        }
    }
}
