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
    using System.Diagnostics;

    /// <summary>
    ///   Returns the current version of the bot.
    /// </summary>
    internal class Version : GenericCommand
    {
        public Version(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public string version
        {
            get { return "6.1"; }
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            if (GlobalFunctions.isInArray("@svn", args) != -1)
                return new CommandResponseHandler(getVersionString());
            return new CommandResponseHandler(this.version);
        }

        /// <summary>
        /// Gets the version string.
        /// </summary>
        /// <returns></returns>
        public string getVersionString()
        {
            string rev = Process.Start("svnversion").StandardOutput.ReadLine();

            string versionString = version + "-r" + rev;

            return versionString;
        }
    }
}