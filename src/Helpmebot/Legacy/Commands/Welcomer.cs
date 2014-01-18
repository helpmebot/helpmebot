// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Welcomer.cs" company="Helpmebot Development Team">
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
//   Controls the newbie welcomer
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    using Helpmebot;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using NHibernate.Criterion;
    using NHibernate.Linq;

    /// <summary>
    /// Controls the newbie welcomer
    /// </summary>
    internal class Welcomer : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Welcomer"/> class.
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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Welcomer(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var response = new CommandResponseHandler();

            if (this.Arguments.Length == 0)
            {
                response.respond(this.MessageService.NotEnoughParameters(this.Channel, "Welcomer", 1, 0));
                return response;
            }

            // TODO: fix me
            var repository = ServiceLocator.Current.GetInstance<IWelcomeUserRepository>();

            List<string> argumentsList = this.Arguments.ToList();
            var mode = argumentsList.PopFromFront();

            switch (mode.ToLower())
            {
                case "enable":
                case "disable":
                    response.respond(
                        this.MessageService.RetrieveMessage("Welcomer-ObsoleteOption", this.Channel, new[] { mode }),
                        CommandResponseDestination.PrivateMessage);
                    break;
                case "add":
                    var welcomeUser = new WelcomeUser
                                          {
                                              Nick = ".*",
                                              User = ".*",
                                              Host = string.Join(" ", argumentsList.ToArray()),
                                              Channel = this.Channel,
                                              Exception = false
                                          };
                    repository.Save(welcomeUser);

                    response.respond(this.MessageService.Done(this.Channel));
                    break;
                case "del":
                case "Delete":
                case "remove":

                    this.Log.Debug("Getting list of welcomeusers ready for deletion!");

                    // TODO: move to repository.
                    var criteria = Restrictions.And(
                        Restrictions.Eq("Host", string.Join(" ", argumentsList.ToArray())),
                        Restrictions.Eq("Channel", this.Channel));

                    var welcomeUsers = repository.Get(criteria);

                    this.Log.Debug("Got list of WelcomeUsers, proceeding to Delete...");

                    repository.Delete(welcomeUsers);

                    this.Log.Debug("All done, cleaning up and sending message to IRC");

                    response.respond(this.MessageService.Done(this.Channel));
                    break;
                case "list":
                    var welcomeForChannel = repository.GetWelcomeForChannel(this.Channel);
                    welcomeForChannel.ForEach(x => response.respond(x.Host));
                    break;
            }
            
            return response;
        }
    }
}
