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
#if !DEBUG
    using System;
#endif

    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;

    using NHibernate.Cfg;

    using Environment = System.Environment;

    /// <summary>
    ///   Returns the current version of the bot.
    /// </summary>
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
            
            System.Version version = Assembly.GetExecutingAssembly().GetName().Version;

#if !DEBUG
            var date = new DateTime(2000, 1, 1, 0, 0, 0);
            date = date.AddDays(version.Build);
            date = date.AddSeconds(version.Revision * 2);
#endif
            var messageArgs = new List<string>
                                  {
                                      version.Major.ToString(CultureInfo.InvariantCulture),
                                      version.Minor.ToString(CultureInfo.InvariantCulture),
#if DEBUG
                                      "*",
                                      "*",
                                      "DEBUG",
#else
                                      version.Build.ToString(CultureInfo.InvariantCulture),
                                      version.Revision.ToString(CultureInfo.InvariantCulture),
                                      date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
#endif
                                      Environment.OSVersion.ToString(),
                                      Environment.Version.ToString()
                                  };

            string message = this.CommandServiceHelper.MessageService.RetrieveMessage("CmdVersion", this.Channel, messageArgs);

            return new CommandResponseHandler(message);
        }
    }
}