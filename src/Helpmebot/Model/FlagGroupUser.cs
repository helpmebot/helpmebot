// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupUser.cs" company="Helpmebot Development Team">
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
//   The flag group user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Model
{
    using Helpmebot.Persistence;

    /// <summary>
    /// The flag group user.
    /// </summary>
    public class FlagGroupUser : EntityBase
    {
        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public virtual string Nickname { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public virtual string Username { get; set; }

        /// <summary>
        /// Gets or sets the hostname.
        /// </summary>
        public virtual string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        public virtual string Account { get; set; }

        /// <summary>
        /// Gets or sets the flag group.
        /// </summary>
        public virtual FlagGroup FlagGroup { get; set; }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                @"{0}!{1}@{2} ({3}): {4}",
                this.Nickname,
                this.Username,
                this.Hostname,
                this.Account,
                this.FlagGroup);
        }
    }
}
