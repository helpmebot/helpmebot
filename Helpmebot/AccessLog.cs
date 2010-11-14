// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System;
using System.Reflection;
using helpmebot6.Commands;

#endregion

namespace helpmebot6
{
    /// <summary>
    /// Represents the bot access log.
    /// </summary>
    internal class AccessLog
    {
        private static AccessLog _instance;

        /// <summary>
        /// Returns the instance.
        /// </summary>
        /// <returns></returns>
        public static AccessLog instance()
        {
            return _instance ?? ( _instance = new AccessLog( ) );
        }

        protected AccessLog()
        {
        }

        /// <summary>
        /// Saves the specified log entry.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        public bool save(AccessLogEntry logEntry)
        {
            DAL.singleton().insert("accesslog", "", logEntry.alUser.ToString(), logEntry.alUser.accessLevel.ToString(),
                                   logEntry.alReqaccesslevel.ToString(), "", logEntry.alClass.ToString(),
                                   (logEntry.alAllowed ? "1" : "0"), logEntry.alChannel, logEntry.alParams);
								   return true;
        }

        /// <summary>
        /// Represents an access log entry
        /// </summary>
        public struct AccessLogEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AccessLogEntry"/> struct.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <param name="command">The command.</param>
            /// <param name="success">if set to <c>true</c> [success].</param>
            /// <param name="channel">The channel the command was launched from</param>
            /// <param name="parameters"></param>
            public AccessLogEntry(User source, Type command, bool success, string channel, string[] parameters)
            {
                this._alId = 0;
                this._alDate = new DateTime(0);
                this._alUser = source;
                this._alClass = command;
                this._alAllowed = success;
                this._alReqaccesslevel = ((GenericCommand) Activator.CreateInstance(this._alClass)).accessLevel;
                this._channel = channel;
                this._params = string.Join(" ", parameters) ?? string.Empty;
            }

            private readonly int _alId;
            private readonly User _alUser;
            private readonly User.UserRights _alReqaccesslevel;
            private readonly Type _alClass;
            private readonly DateTime _alDate;
            private readonly bool _alAllowed;
            private readonly string _channel;
            private readonly string _params;

            /// <summary>
            /// Gets the access log id.
            /// </summary>
            /// <value>The al id.</value>
            public int alId
            {
                get { return this._alId; }
            }

            /// <summary>
            /// Gets the access log user.
            /// </summary>
            /// <value>The al user.</value>
            public User alUser
            {
                get { return this._alUser; }
            }

            /// <summary>
            /// Gets the access log required access level.
            /// </summary>
            /// <value>The al reqaccesslevel.</value>
            public User.UserRights alReqaccesslevel
            {
                get { return this._alReqaccesslevel; }
            }

            /// <summary>
            /// Gets the access log command class.
            /// </summary>
            /// <value>The al class.</value>
            public Type alClass
            {
                get { return this._alClass; }
            }

            /// <summary>
            /// Gets the access log date.
            /// </summary>
            /// <value>The al date.</value>
            public DateTime alDate
            {
                get { return this._alDate; }
            }

            /// <summary>
            /// Gets a value indicating whether this entry was allowed.
            /// </summary>
            /// <value><c>true</c> if allowed; otherwise, <c>false</c>.</value>
            public bool alAllowed
            {
                get { return this._alAllowed; }
            }

            public string alChannel
            {
                get { return _channel; }
            }

            public string alParams
            {
                get { return _params; }
            }
        }

        /// <summary>
        /// Does the flood check.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the user is flooding; otherwise <c>false</c></returns>
        public bool doFloodCheck(User source)
        {
            //TODO: Implement
            return false;
        }
    }
}