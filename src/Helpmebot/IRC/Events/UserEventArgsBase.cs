// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserEventArgsBase.cs" company="Helpmebot Development Team">
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
//   Defines the UserEventArgsBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Events
{
    using Helpmebot.IRC.Messages;
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The user event args base.
    /// </summary>
    public class UserEventArgsBase : MessageReceivedEventArgs
    {
        /// <summary>
        /// The user.
        /// </summary>
        private readonly IUser user;

        /// <summary>
        /// Initialises a new instance of the <see cref="UserEventArgsBase"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        public UserEventArgsBase(IMessage message, IUser user)
            : base(message)
        {
            this.user = user;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public IUser User
        {
            get
            {
                return this.user;
            }
        }
    }
}