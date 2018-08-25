// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyUser.cs" company="Helpmebot Development Team">
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

using Stwalkerster.IrcClient.Model.Interfaces;

namespace Helpmebot.Legacy.Model
{
    using System;
    using Castle.Core.Logging;
    using Helpmebot.Legacy.Database;
    using Microsoft.Practices.ServiceLocation;
    using MySql.Data.MySqlClient;

    /// <summary>
    ///     The user.
    /// </summary>
    public class LegacyUser 
    {
        private LegacyUserRights accessLevel;
        private bool hasRetrievedAccessLevel;
        private ILogger log;

        public LegacyUserRights AccessLevel
        {
            get
            {
                try
                {
                    if (this.hasRetrievedAccessLevel == false)
                    {
                        // FIXME: ServiceLocator - legacydatabase
                        var db = ServiceLocator.Current.GetInstance<ILegacyDatabase>();
                        
                        var command =
                            new MySqlCommand(
                                "SELECT user_accesslevel FROM `user` WHERE @nick LIKE user_nickname AND @user LIKE user_username AND @host LIKE user_hostname ORDER BY `user_accesslevel` ASC;");
                        command.Parameters.AddWithValue("@nick", this.Nickname);
                        command.Parameters.AddWithValue("@user", this.Username);
                        command.Parameters.AddWithValue("@host", this.Hostname);

                        string accesslevel = db.ExecuteScalarSelect(command);

                        if (string.IsNullOrEmpty(accesslevel))
                        {
                            accesslevel = "Normal";
                        }

                        var ret = (LegacyUserRights)Enum.Parse(typeof(LegacyUserRights), accesslevel);

                        this.accessLevel = ret;
                        this.hasRetrievedAccessLevel = true;
                        return ret;
                    }

                    return this.accessLevel;
                }
                catch (Exception ex)
                {
                    this.log.Error(ex.Message, ex);
                }

                return LegacyUserRights.Normal;
            }
        }

        public string Account { get; set; }
        public string Hostname { get; set; }
        public string Nickname { get; set; }
        public string Username { get; set; }

        public static LegacyUser NewFromOtherUser(IUser source)
        {
            if (source.GetType() == typeof(LegacyUser))
            {
                return (LegacyUser)source;
            }
            
            return NewFromString(string.Format("{0}!{1}@{2}", source.Nickname, source.Username, source.Hostname));
        }

        public static LegacyUser NewFromString(string source)
        {
            string user, host;
            string nick = user = host = null;

            var logger = ServiceLocator.Current.GetInstance<ILogger>();

            try
            {
                if (source.Contains("@") && source.Contains("!"))
                {
                    char[] splitSeparators = { '!', '@' };
                    string[] sourceSegment = source.Split(splitSeparators, 3);
                    nick = sourceSegment[0];
                    user = sourceSegment[1];
                    host = sourceSegment[2];
                }
                else if (source.Contains("@"))
                {
                    char[] splitSeparators = { '@' };
                    string[] sourceSegment = source.Split(splitSeparators, 2);
                    nick = sourceSegment[0];
                    host = sourceSegment[1];
                }
                else
                {
                    nick = source;
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                logger.Error(ex.Message, ex);
            }

            if (nick == null)
            {
                return null;
            }

            var ret = new LegacyUser { Hostname = host, Nickname = nick, Username = user, log = logger };
            return ret;
        }

        /// <summary>
        ///     Recompiles the source string
        /// </summary>
        /// <returns>nick!user@host, OR nick@host, OR nick</returns>
        public override string ToString()
        {
            string endResult = string.Empty;

            if (this.Nickname != null)
            {
                endResult = this.Nickname;
            }

            if (this.Username != null)
            {
                endResult += "!" + this.Username;
            }

            if (this.Hostname != null)
            {
                endResult += "@" + this.Hostname;
            }

            return endResult;
        }
    }
}