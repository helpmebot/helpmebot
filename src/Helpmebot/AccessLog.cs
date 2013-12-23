// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessLog.cs" company="Helpmebot Development Team">
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
//   Represents the bot access log.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;

    using helpmebot6.Commands;

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
            return DAL.singleton().insert("accesslog", "", logEntry.alUser.ToString(), logEntry.alUser.accessLevel.ToString(),
                                          logEntry.alReqaccesslevel.ToString(), "", logEntry.alClass.ToString(),
                                          (logEntry.alAllowed ? "1" : "0"), logEntry.alChannel, logEntry.alParams) != -1;
        }

        /// <summary>
        /// Represents an access log entry
        /// </summary>
        public struct AccessLogEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AccessLogEntry"/> struct.
            /// </summary>
            /// <param name="source">
            /// The source.
            /// </param>
            /// <param name="command">
            /// The command.
            /// </param>
            /// <param name="success">
            /// if set to <c>true</c> [success].
            /// </param>
            /// <param name="channel">
            /// The channel the command was launched from
            /// </param>
            /// <param name="parameters">
            /// </param>
            /// <param name="requiredAccessLevel">
            /// The required Access Level.
            /// </param>
            public AccessLogEntry(LegacyUser source, Type command, bool success, string channel, string[] parameters, LegacyUser.UserRights requiredAccessLevel)
            {
                this._alId = 0;
                this._alDate = new DateTime(0);
                this._alUser = source;
                this._alClass = command;
                this._alAllowed = success;
                this._alReqaccesslevel = requiredAccessLevel;
                this._channel = channel;
                this._params = string.Join(" ", parameters);
            }

            private  int _alId;
            private  LegacyUser _alUser;
            private  LegacyUser.UserRights _alReqaccesslevel;
            private  Type _alClass;
            private  DateTime _alDate;
            private  bool _alAllowed;
            private  string _channel;
            private  string _params;

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
            public LegacyUser alUser
            {
                get { return this._alUser; }
            }

            /// <summary>
            /// Gets the access log required access level.
            /// </summary>
            /// <value>The al reqaccesslevel.</value>
            public LegacyUser.UserRights alReqaccesslevel
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
                get { return this._channel; }
            }

            public string alParams
            {
                get { return this._params; }
            }

            public static AccessLogEntry[] get(params DAL.WhereConds[] conditions)
            {
                DAL.Select q = new DAL.Select("*");
                q.addWhere(conditions);
                q.setFrom("accesslog");

                List<string> columns;
                ArrayList al = DAL.singleton().executeSelect(q, out columns);

                AccessLogEntry[] entries = new AccessLogEntry[al.Count];

                for (int j = 0; j < al.Count; j++)
                {


                    string[] row = (string[]) al[j];

                    AccessLogEntry entry = new AccessLogEntry();

                    string usermask = string.Empty;
                    LegacyUser.UserRights useraccess = LegacyUser.UserRights.Normal;
                    #region parse
                    for (int i = 0; i < row.Length; i++)
                    {

                        switch (columns[i])
                        {
                            case ACCESSLOG_ID:
                                entry._alId = int.Parse(row[i]);
                                break;
                            case ACCESSLOG_USER:
                                usermask = row[i];
                                break;
                            case ACCESSLOG_USER_ACCESS:
                                useraccess = (LegacyUser.UserRights) Enum.Parse(typeof (LegacyUser.UserRights), row[i]);
                                break;
                            case ACCESSLOG_COMMAND_ACCESS:
                                entry._alReqaccesslevel = (LegacyUser.UserRights) Enum.Parse(typeof (LegacyUser.UserRights), row[i]);
                                break;
                            case ACCESSLOG_DATE:
                                entry._alDate = DateTime.Parse(row[i]);
                                break;
                            case ACCESSLOG_COMMAND_CLASS:
                                entry._alClass = Type.GetType(row[i]);
                                break;
                            case ACCESSLOG_ALLOWED:
                                entry._alAllowed = row[i] == "0" ? false : true;
                                break;
                            case ACCESSLOG_CHANNEL:
                                entry._channel = row[i];
                                break;
                            case ACCESSLOG_ARGS:
                                entry._params = row[i];
                                break;
                        }

                        entry._alUser = LegacyUser.newFromStringWithAccessLevel(usermask, useraccess);

                    }
                    #endregion

                    entries[j] = entry;

                }
                return entries;
            }

            private const string ACCESSLOG_ID = "al_id";
            private const string ACCESSLOG_USER = "al_nuh";
            private const string ACCESSLOG_USER_ACCESS = "al_accesslevel";
            private const string ACCESSLOG_COMMAND_ACCESS = "al_reqaccesslevel";
            private const string ACCESSLOG_DATE = "al_date";
            private const string ACCESSLOG_COMMAND_CLASS = "al_class";
            private const string ACCESSLOG_ALLOWED = "al_allowed";
            private const string ACCESSLOG_CHANNEL = "al_channel";
            private const string ACCESSLOG_ARGS = "al_args";
        }

            /// <summary>
        /// Does the flood check.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the user is flooding; otherwise <c>false</c></returns>
        public bool doFloodCheck(LegacyUser source)
        {
            //TODO: Implement
            return false;
        }



        public AccessLogEntry[] get(params DAL.WhereConds[] conditions)
        {
            return AccessLogEntry.get(conditions);
        }
    }
}