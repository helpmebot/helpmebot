// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyUser.cs" company="Helpmebot Development Team">
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
// --------------------------------------------------------------------------------------------------------------------

using Stwalkerster.IrcClient.Model.Interfaces;

namespace Helpmebot.Legacy.Model
{
    using System;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model;

    /// <summary>
    ///     The user.
    /// </summary>
    [Obsolete("Use IrcUser")]
    public class LegacyUser
    {
        public string Hostname { get; set; }
        public string Nickname { get; set; }
        public string Username { get; set; }

        public IUser ToIrcUser(IIrcClient client)
        {
            return new IrcUser(client)
            {
                Nickname = this.Nickname,
                Username = this.Username,
                Hostname = this.Hostname,
                SkeletonStatus = IrcUserSkeletonStatus.PrefixOnly
            };
        }
        
        public static LegacyUser NewFromOtherUser(IUser source)
        {
            if (source.GetType() == typeof(LegacyUser))
            {
                return (LegacyUser)source;
            }

            return new LegacyUser
            {
                Hostname = source.Hostname, Username = source.Username, Nickname = source.Nickname
            };
        }
        
        /// <summary>
        ///     Recompiles the source string
        /// </summary>
        /// <returns>nick!user@host, OR nick@host, OR nick</returns>
        public override string ToString()
        {
            string endResult = string.Empty;

            if (this.Nickname != null)
            {
                endResult = this.Nickname;
            }

            if (this.Username != null)
            {
                endResult += "!" + this.Username;
            }

            if (this.Hostname != null)
            {
                endResult += "@" + this.Hostname;
            }

            return endResult;
        }
    }
}