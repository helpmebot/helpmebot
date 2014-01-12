// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcUser.cs" company="Helpmebot Development Team">
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
//   Defines the IrcUser type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Model
{
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The IRC user.
    /// </summary>
    public class IrcUser : IUser
    {
        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the hostname.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether away.
        /// </summary>
        public bool Away { get; set; }
        
        /// <summary>
        /// The from prefix.
        /// </summary>
        /// <param name="prefix">
        /// The prefix.
        /// </param>
        /// <returns>
        /// The <see cref="IrcUser"/>.
        /// </returns>
        public static IrcUser FromPrefix(string prefix)
        {
            string nick;
            string user = null;
            string host = null;
            if (prefix.Contains("@"))
            {
                var indexOfAt = prefix.IndexOf('@');

                host = prefix.Substring(indexOfAt + 1);
                if (prefix.Contains("!"))
                {
                    var indexOfBang = prefix.IndexOf('!');

                    user = prefix.Substring(indexOfBang + 1, indexOfAt - (indexOfBang + 1));
                    nick = prefix.Substring(0, indexOfBang);
                }
                else
                {
                    nick = prefix.Substring(0, indexOfAt);
                }
            }
            else
            {
                nick = prefix;
            }

            return new IrcUser { Hostname = host, Username = user, Nickname = nick };
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Account) || this.Account == "*")
            {
                return string.Format("{0}!{1}@{2}", this.Nickname, this.Username, this.Hostname);
            }

            return string.Format("{0} [{1}!{2}@{3}]", this.Account, this.Nickname, this.Username, this.Hostname);
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The object.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((IrcUser)obj);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Nickname != null ? this.Nickname.GetHashCode() : 0;
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected bool Equals(IrcUser other)
        {
            return string.Equals(this.Nickname, other.Nickname);
        }
    }
}
