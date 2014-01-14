// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcAccessLayer.cs" company="Helpmebot Development Team">
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
//   IRC Access Layer
//   Provides an interface to IRC.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Legacy.IRC
{
    using Helpmebot.Legacy.Model;

    /// <summary>
    ///   IRC Access Layer - Provides an interface to IRC.
    /// </summary>
    public sealed class IrcAccessLayer
    {
        #region delegates

        /// <summary>
        /// The join event handler.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public delegate void JoinEventHandler(LegacyUser source, string channel);

        /// <summary>
        /// The invite event handler.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public delegate void InviteEventHandler(LegacyUser source, string nickname, string channel);

        #endregion
    }
}