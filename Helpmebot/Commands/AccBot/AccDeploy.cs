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
    using System.IO;
    using System.Web;

    using HttpRequest = helpmebot6.HttpRequest;

    /// <summary>
    /// The deploy ACC command.
    /// </summary>
    internal class Accdeploy : GenericCommand
    {
        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] args = this.Arguments;

            Helpmebot6.irc.ircPrivmsg(this.Channel, new Message().get("DeployInProgress"));

            string revision;

            bool showUrl = false;

            if (args[0].ToLower() == "@url")
            {
                showUrl = true;
                GlobalFunctions.popFromFront(ref args);
            }

            if (args.Length > 0 && args[0] != string.Empty)
            {
                revision = string.Join(" ", args);
            }
            else
            {
                throw new ArgumentException();
            }
            
            string apiDeployPassword = Configuration.singleton()["accDeployPassword"];

            string key = this.EncodeMD5(this.EncodeMD5(revision) + apiDeployPassword);

            revision = HttpUtility.UrlEncode(revision);

            string requestUri = "http://toolserver.org/~acc/deploy/deploy.php?r=" + revision + "&k=" + key;

            var r = new StreamReader(HttpRequest.get(requestUri, 1000 * 30 /* 30 sec timeout */));

            var crh = new CommandResponseHandler();
            if (showUrl)
            {
                crh.respond(requestUri, CommandResponseDestination.PrivateMessage);
            }

            foreach (var x in r.ReadToEnd().Split('\n', '\r'))
            {
                crh.respond(x);
            }

            return crh;
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
