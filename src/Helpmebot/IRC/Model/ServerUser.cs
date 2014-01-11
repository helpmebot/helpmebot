// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerUser.cs" company="Helpmebot Development Team">
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
//   Defines the ServerUser type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Model
{
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The server user.
    /// </summary>
    public class ServerUser : IUser
    {
        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public string 
            Nickname
        {
            get
            {
                return string.Empty;
            }
            
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username
        {
            get
            {
                return string.Empty;
            }
     
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the hostname.
        /// </summary>
        public string Hostname
        {
            get
            {
                return string.Empty;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        public string Account
        {
            get
            {
                return string.Empty;
            }

            set
            {
            }
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return "[SERVER]";
        }
    }
}
