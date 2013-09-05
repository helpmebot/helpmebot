// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Oauth.cs" company="Helpmebot Development Team">
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
//   Performs authorisation to Twitter
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Reflection;

    /// <summary>
    /// Performs authorisation to Twitter
    /// </summary>
    internal class Oauth : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Oauth"/> class.
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
        public Oauth(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            Logger.instance()
                .addToLog(
                    "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                    Logger.LogTypes.DNWB);

            if (this.Arguments.Length == 1 && this.Arguments[0] != string.Empty)
            {
                new Twitter().authorise(this.Arguments[0]);
            }
            else
            {
                // TODO: verify this!
                new Twitter();
            }

            return new CommandResponseHandler(new Message().get("done"));
        }
    }
}