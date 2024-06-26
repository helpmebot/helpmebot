﻿namespace Helpmebot.Commands.Commands.FunCommands
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public abstract class ProtectedTargetedFunCommandBase : TargetedFunCommandBase
    {
        private readonly List<string> forbiddenTargets = new List<string> {"itself", "himself", "herself", "themself"};

        protected ProtectedTargetedFunCommandBase(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client,
            responder,
            channelManagementService)
        {
        }

        protected override string CommandTarget
        {
            get
            {
                if (this.forbiddenTargets.Contains(base.CommandTarget.ToLower()))
                {
                    return this.User.Nickname;
                }

                if (base.CommandTarget.ToLower() == this.Client.Nickname.ToLower())
                {
                    return this.User.Nickname;
                }

                return base.CommandTarget;
            }
        }
    }
}