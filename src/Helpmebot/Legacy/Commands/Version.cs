// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Version.cs" company="Helpmebot Development Team">
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
//   Returns the current version of the bot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Stwalkerster.IrcClient;

    /// <summary>
    ///   Returns the current version of the bot.
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Version : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Version"/> class.
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
        public Version(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var ircVersion = this.GetFileVersion(Assembly.GetAssembly(typeof(IrcClient)));

            var messageArgs = new List<string>
                                  {
                                      version.Major.ToString(CultureInfo.InvariantCulture),
                                      version.Minor.ToString(CultureInfo.InvariantCulture),
                                      version.Build.ToString(CultureInfo.InvariantCulture),
                                      ircVersion
                                  };

            string messageFormat = "Version {0}.{1} (Build {2}), using Stwalkerster.IrcClient v{3}";
            string message = string.Format(messageFormat, messageArgs.ToArray());

            return new CommandResponseHandler(message);
        }
        
        private string GetFileVersion(Assembly assembly)
        {
            return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        }
    }
}