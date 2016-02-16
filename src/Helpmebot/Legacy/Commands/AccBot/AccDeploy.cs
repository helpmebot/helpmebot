// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccDeploy.cs" company="Helpmebot Development Team">
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
//   Defines the Accdeploy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;

    using HttpRequest = Helpmebot.HttpRequest;

    /// <summary>
    /// The deploy ACC command.
    /// </summary>
    internal class Accdeploy : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Accdeploy"/> class.
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
        public Accdeploy(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var args = this.Arguments;

            var messageService = this.CommandServiceHelper.MessageService;

            var deployInProgressMessage = messageService.RetrieveMessage("DeployInProgress", this.Channel, null); 
            this.CommandServiceHelper.Client.SendMessage(this.Channel, deployInProgressMessage);

            string revision;

            if (args.Length <= 0 || args[0] == string.Empty)
            {
                string[] messageParameters = { "accdeploy", "1", args.Length.ToString(CultureInfo.InvariantCulture) };
                this.CommandServiceHelper.Client.SendNotice(
                    this.Source.Nickname,
                    messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));

                return null;
            }

            revision = string.Join(" ", args);

            string apiDeployPassword = LegacyConfig.Singleton()["accDeployPassword"];

            string key = this.EncodeMD5(this.EncodeMD5(revision) + apiDeployPassword);

            revision = HttpUtility.UrlEncode(revision);

            string requestUri = "http://accounts-dev.wmflabs.org/deploy/deploy.php?r=" + revision + "&k=" + key;

            using (Stream data = HttpRequest.Get(requestUri, 1000 * 30 /* 30 sec timeout */).ToStream())
            {
                var r = new StreamReader(data);

                var crh = new CommandResponseHandler();

                foreach (var x in r.ReadToEnd().Split('\n', '\r'))
                {
                    crh.Respond(x);
                }

                return crh;
            }
        }

        #endregion

        /// <summary>
        /// The md 5.
        /// </summary>
        /// <param name="s">
        /// The s.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string EncodeMD5(string s)
        {
            return
                BitConverter.ToString(
                    System.Security.Cryptography.MD5.Create().ComputeHash(new System.Text.UTF8Encoding().GetBytes(s)))
                    .Replace("-", string.Empty)
                    .ToLower();
        }
    }
}
