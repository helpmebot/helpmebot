// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JoinMessageService.cs" company="Helpmebot Development Team">
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
//   Defines the JoinMessageService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Castle.Core.Logging;

    using Helpmebot.Legacy.IRC;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The join message service.
    /// </summary>
    public class JoinMessageService : IJoinMessageService
    {
        /// <summary>
        /// The IRC network.
        /// </summary>
        private readonly IIrcAccessLayer ircNetwork;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The repository.
        /// </summary>
        private readonly IWelcomeUserRepository repository;

        /// <summary>
        /// The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// Initialises a new instance of the <see cref="JoinMessageService"/> class.
        /// </summary>
        /// <param name="ircNetwork">
        /// The IRC network.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public JoinMessageService(IIrcAccessLayer ircNetwork, ILogger logger, IWelcomeUserRepository repository, IMessageService messageService)
        {
            this.ircNetwork = ircNetwork;
            this.logger = logger;
            this.repository = repository;
            this.messageService = messageService;
        }

        /// <summary>
        /// The welcome.
        /// </summary>
        /// <param name="networkUser">
        /// The network User.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void Welcome(IUser networkUser, string channel)
        {
            List<WelcomeUser> users = this.repository.GetWelcomeForChannel(channel).ToList();
            if (users.Any())
            {
                foreach (var welcomeUser in users)
                {
                    Match nick = new Regex(welcomeUser.Nick).Match(networkUser.nickname);
                    Match user = new Regex(welcomeUser.User).Match(networkUser.username);
                    Match host = new Regex(welcomeUser.Host).Match(networkUser.hostname);

                    if (nick.Success && user.Success && host.Success)
                    {
                        var welcomeMessage = this.messageService.RetrieveMessage(
                            "WelcomeMessage",
                            channel,
                            new[] { networkUser.nickname, channel });

                        this.logger.DebugFormat("Welcoming {0} in channel {1}", networkUser, channel);

                        this.ircNetwork.IrcPrivmsg(channel, welcomeMessage);
                    }
                    else
                    {
                        this.logger.InfoFormat("Unable to find welcomer match for {0} in channel {1}", networkUser, channel);
                    }
                }
            }
            else
            {
                this.logger.InfoFormat("No welcome definitions for {0} were found.", channel);
            }
        }
    }
}
