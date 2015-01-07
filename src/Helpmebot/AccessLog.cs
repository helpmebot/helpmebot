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
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot
{
    using System;

    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;

    using Microsoft.Practices.ServiceLocation;

    using MySql.Data.MySqlClient;

    /// <summary>
    ///     Represents the bot access log.
    /// </summary>
    internal class AccessLog
    {
        #region Static Fields

        /// <summary>
        /// The _instance.
        /// </summary>
        private static AccessLog instance;

        /// <summary>
        /// The legacy database.
        /// </summary>
        private ILegacyDatabase legacyDatabase;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="AccessLog"/> class.
        /// </summary>
        protected AccessLog()
        {
            // FIXME: ServiceLocator - legacydatabase
            this.legacyDatabase = ServiceLocator.Current.GetInstance<ILegacyDatabase>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns the instance.
        /// </summary>
        /// <returns>
        /// The <see cref="AccessLog"/>.
        /// </returns>
        public static AccessLog Instance()
        {
            return instance ?? (instance = new AccessLog());
        }

        /// <summary>
        /// Saves the specified log entry.
        /// </summary>
        /// <param name="logEntry">
        /// The log entry.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Save(AccessLogEntry logEntry)
        {
            var insertCommand =
                new MySqlCommand(
                    "INSERT INTO accesslog (al_nuh, al_accesslevel, al_reqaccesslevel, al_class,"
                    + " al_allowed, al_channel, al_args) VALUES (@nuh, @accesslevel, "
                    + "@reqaccesslevel, @class, @allowed, @channel, @args);");

            insertCommand.Parameters.AddWithValue("@nuh", logEntry.User.ToString());
            insertCommand.Parameters.AddWithValue("@accesslevel", logEntry.User.AccessLevel.ToString());
            insertCommand.Parameters.AddWithValue("@reqaccesslevel", logEntry.RequiredAccessLevel.ToString());
            insertCommand.Parameters.AddWithValue("@class", logEntry.Class.ToString());
            insertCommand.Parameters.AddWithValue("@allowed", logEntry.Allowed);
            insertCommand.Parameters.AddWithValue("@channel", logEntry.Channel);
            insertCommand.Parameters.AddWithValue("@args", logEntry.Parameters);

            this.legacyDatabase.ExecuteCommand(insertCommand);

            return true;
        }

        #endregion

        /// <summary>
        ///     Represents an access log entry
        /// </summary>
        public struct AccessLogEntry
        {
            #region Fields

            /// <summary>
            /// The allowed.
            /// </summary>
            private readonly bool allowed;

            /// <summary>
            /// The class.
            /// </summary>
            private readonly Type @class;

            /// <summary>
            /// The date.
            /// </summary>
            private readonly DateTime date;

            /// <summary>
            /// The id.
            /// </summary>
            private readonly int id;

            /// <summary>
            /// The required access level.
            /// </summary>
            private readonly LegacyUser.UserRights requiredAccessLevel;

            /// <summary>
            /// The user.
            /// </summary>
            private readonly LegacyUser user;

            /// <summary>
            /// The _channel.
            /// </summary>
            private readonly string channel;

            /// <summary>
            /// The parameters.
            /// </summary>
            private readonly string parameters;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initialises a new instance of the <see cref="AccessLogEntry"/> struct. 
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
            /// The parameters.
            /// </param>
            /// <param name="requiredAccessLevel">
            /// The required Access Level.
            /// </param>
            public AccessLogEntry(
                LegacyUser source, 
                Type command, 
                bool success, 
                string channel, 
                string[] parameters, 
                LegacyUser.UserRights requiredAccessLevel)
            {
                this.id = 0;
                this.date = new DateTime(0);
                this.user = source;
                this.@class = command;
                this.allowed = success;
                this.requiredAccessLevel = requiredAccessLevel;
                this.channel = channel;
                this.parameters = string.Join(" ", parameters);
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     Gets a value indicating whether this entry was allowed.
            /// </summary>
            /// <value><c>true</c> if allowed; otherwise, <c>false</c>.</value>
            public bool Allowed
            {
                get
                {
                    return this.allowed;
                }
            }

            /// <summary>
            /// Gets the al channel.
            /// </summary>
            public string Channel
            {
                get
                {
                    return this.channel;
                }
            }

            /// <summary>
            ///     Gets the access log command class.
            /// </summary>
            /// <value>The al class.</value>
            public Type Class
            {
                get
                {
                    return this.@class;
                }
            }

            /// <summary>
            ///     Gets the access log date.
            /// </summary>
            /// <value>The al date.</value>
            public DateTime Date
            {
                get
                {
                    return this.date;
                }
            }

            /// <summary>
            ///     Gets the access log id.
            /// </summary>
            /// <value>The al id.</value>
            public int Id
            {
                get
                {
                    return this.id;
                }
            }

            /// <summary>
            /// Gets the parameters.
            /// </summary>
            public string Parameters
            {
                get
                {
                    return this.parameters;
                }
            }

            /// <summary>
            ///     Gets the access log required access level.
            /// </summary>
            /// <value>The required access level.</value>
            public LegacyUser.UserRights RequiredAccessLevel
            {
                get
                {
                    return this.requiredAccessLevel;
                }
            }

            /// <summary>
            ///     Gets the access log user.
            /// </summary>
            /// <value>The user.</value>
            public LegacyUser User
            {
                get
                {
                    return this.user;
                }
            }

            #endregion
        }
    }
}