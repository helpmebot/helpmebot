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
    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Model;
    using Helpmebot.Monitoring;
    using Helpmebot.Services.Interfaces;

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
        public Welcomer(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var args = this.Arguments;

            var ignore = false;
            switch (args[0].ToLower())
            {
                case "enable":
                    if (LegacyConfig.singleton()["welcomeNewbie", this.Channel] == "true")
                    {
                        return new CommandResponseHandler(new Message().GetMessage("no-change"));
                    }

                    LegacyConfig.singleton()["welcomeNewbie", this.Channel] = "true";
                    return new CommandResponseHandler(this.MessageService.RetrieveMessage(Messages.Done, this.Channel, null));
                case "disable":
                    if (LegacyConfig.singleton()["welcomeNewbie", this.Channel] == "false")
                    {
                        return new CommandResponseHandler(new Message().GetMessage("no-change"));
                    }

                    LegacyConfig.singleton()["welcomeNewbie", this.Channel] = "false";
                    return new CommandResponseHandler(this.MessageService.RetrieveMessage(Messages.Done, this.Channel, null));
                case "global":
                    LegacyConfig.singleton()["welcomeNewbie", this.Channel] = null;
                    return new CommandResponseHandler(new Message().GetMessage("defaultSetting"));
                case "add":
                    if (args[1] == "@ignore")
                    {
                        ignore = true;
                        GlobalFunctions.popFromFront(ref args);
                    }

                    NewbieWelcomer.Instance().AddHost(args[1], ignore);
                    return new CommandResponseHandler(this.MessageService.RetrieveMessage(Messages.Done, this.Channel, null));
                case "del":
                    if (args[1] == "@ignore")
                    {
                        ignore = true;
                        GlobalFunctions.popFromFront(ref args);
                    }

                    NewbieWelcomer.Instance().DeleteHost(args[1], ignore);
                    return new CommandResponseHandler(this.MessageService.RetrieveMessage(Messages.Done, this.Channel, null));
                case "list":
                    if (args[1] == "@ignore")
                    {
                        ignore = true;
                        GlobalFunctions.popFromFront(ref args);
                    }

                    var crh = new CommandResponseHandler();
                    string[] list = NewbieWelcomer.Instance().GetHosts(ignore);
                    foreach (string item in list)
                    {
                        crh.respond(item);
                    }

                    return crh;
            }

            return new CommandResponseHandler();
        }
    }
}