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

    class Accdeploy : GenericCommand
    {
        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            Helpmebot6.irc.ircPrivmsg(channel, new Message().get("DeployInProgress"));

            string revision;

            bool showUrl = false;

            if (args[0].ToLower() == "@url")
            {
                showUrl = true;
                GlobalFunctions.popFromFront(ref args);
            }

            if (args.Length > 0 && args[0] != "")
            {
                revision = string.Join(" ", args);
            }
            else
            {
                throw new ArgumentException();
            }
            
            string apiDeployPassword = Configuration.singleton()["accDeployPassword"];

            string key = md5(md5(revision) + apiDeployPassword);

            revision = HttpUtility.UrlEncode(revision);

            string requestUri = "http://toolserver.org/~acc/deploy/deploy.php?r=" + revision + "&k=" + key;

            var r = new StreamReader(HttpRequest.get(requestUri, 1000*30 /* 30 sec timeout */));

            var crh = new CommandResponseHandler();
            if (showUrl)
            {
                crh.respond(requestUri, CommandResponseDestination.PrivateMessage);
            }

            foreach(var x in r.ReadToEnd().Split('\n', '\r')) crh.respond(x);
            return crh;
        }

        #endregion

        string md5(string s)
        {
            return BitConverter.ToString(
                System.Security.Cryptography.MD5.Create().ComputeHash(
                    new System.Text.UTF8Encoding().GetBytes(s)
                )
            ).Replace("-", "").ToLower();
        }
    }
}
