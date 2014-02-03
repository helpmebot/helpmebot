// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Numerics.cs" company="Helpmebot Development Team">
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
//   Defines the Numerics type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Messages
{
    /// <summary>
    /// The IRC numeric commands.
    /// </summary>
    public class Numerics
    {
        /// <summary>
        /// The unavailable resource.
        /// </summary>
        public const string UnavailableResource = "437";

        /// <summary>
        /// The nickname in use.
        /// </summary>
        public const string NicknameInUse = "433";

        /// <summary>
        /// The welcome.
        /// </summary>
        public const string Welcome = "001";

        #region SASL

        /// <summary>
        /// The SASL authentication failed.
        /// </summary>
        public const string SaslAuthFailed = "904";

        /// <summary>
        /// The SASL logged in.
        /// </summary>
        public const string SaslLoggedIn = "900";

        /// <summary>
        /// The SASL authentication succeeded.
        /// </summary>
        public const string SaslSuccess = "903";

        /// <summary>
        /// The SASL authentication was aborted.
        /// </summary>
        public const string SaslAborted = "906";

        #endregion

        /// <summary>
        /// The no channel topic.
        /// </summary>
        public const string NoChannelTopic = "331";

        /// <summary>
        /// The channel topic.
        /// </summary>
        public const string ChannelTopic = "332";

        /// <summary>
        /// The name reply.
        /// </summary>
        public const string NameReply = "353";

        /// <summary>
        /// The end of names.
        /// </summary>
        public const string EndOfNames = "366";

        /// <summary>
        /// The end of who.
        /// </summary>
        public const string EndOfWho = "315";

        /// <summary>
        /// The who x reply.
        /// </summary>
        public const string WhoXReply = "354";
    }
}
