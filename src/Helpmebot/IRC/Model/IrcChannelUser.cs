// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcChannelUser.cs" company="Helpmebot Development Team">
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
//   Defines the IrcChannelUser type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Model
{
    /// <summary>
    /// The channel user.
    /// </summary>
    public class IrcChannelUser
    {
        /// <summary>
        /// The user.
        /// </summary> 
        private readonly IrcUser user;

        /// <summary>
        /// The channel.
        /// </summary>
        private readonly string channel;

        /// <summary>
        /// Initialises a new instance of the <see cref="IrcChannelUser"/> class.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public IrcChannelUser(IrcUser user, string channel)
        {
            this.user = user;
            this.channel = channel;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public IrcUser User
        {
            get
            {
                return this.user;
            }
        }

        /// <summary>
        /// Gets the channel.
        /// </summary>
        public string Channel
        {
            get
            {
                return this.channel;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether operator.
        /// </summary>
        public bool Operator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether voice.
        /// </summary>
        public bool Voice { get; set; }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "[{0} {1}{2} {3}]",
                this.Channel,
                this.Operator ? "@" : string.Empty,
                this.Voice ? "+" : string.Empty,
                this.User);
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

            return this.Equals((IrcChannelUser)obj);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.User != null ? this.User.GetHashCode() : 0) * 397) ^ (this.Channel != null ? this.Channel.GetHashCode() : 0);
            }
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
        protected bool Equals(IrcChannelUser other)
        {
            return object.Equals(this.User, other.User) && string.Equals(this.Channel, other.Channel);
        }
    }
}
