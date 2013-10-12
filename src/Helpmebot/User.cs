// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs" company="Helpmebot Development Team">
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
//   Defines the User type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.Reflection;

    using Helpmebot.Legacy.Database;

    using log4net;

    /// <summary>
    /// The user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The log4net logger for this class
        /// </summary>
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DAL _db;

        private UserRights _accessLevel;
        private bool _retrievedAccessLevel;

        public User()
        {
            this._db = DAL.singleton();
        }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        /// <value>The nickname.</value>
        public string nickname { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string username { get; set; }

        /// <summary>
        /// Gets or sets the hostname.
        /// </summary>
        /// <value>The hostname.</value>
        public string hostname { get; set; }

        /// <summary>
        /// Gets or sets the network.
        /// </summary>
        /// <value>The network.</value>
        public uint network { get; private set; }

        /// <summary>
        /// News from string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static User newFromString(string source)
        {
            return newFromString(source, 0);
        }

        /// <summary>
        /// New user from string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="network">The network.</param>
        /// <returns></returns>
        public static User newFromString(string source, uint network)
        {
            string user, host;
            string nick = user = host = null;
            try
            {
                if ((source.Contains("@")) && (source.Contains("!")))
                {
                    char[] splitSeparators = {'!', '@'};
                    string[] sourceSegment = source.Split(splitSeparators, 3);
                    nick = sourceSegment[0];
                    user = sourceSegment[1];
                    host = sourceSegment[2];
                }
                else if (source.Contains("@"))
                {
                    char[] splitSeparators = {'@'};
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
                Log.Error(ex.Message, ex);
            }

            User ret = new User
                           {
                               hostname = host,
                               nickname = nick,
                               username = user,
                               network = network
                           };
            return ret;
        }

        public static User newFromStringWithAccessLevel(string source, UserRights accessLevel)
        {
            return newFromStringWithAccessLevel(source, 0, accessLevel);
        }

        public static User newFromStringWithAccessLevel(string source, uint network, UserRights accessLevel)
        {
            User u = newFromString(source, network);
            u._accessLevel = accessLevel;
            return u;
        }

        /// <summary>
        ///   Recompiles the source string
        /// </summary>
        /// <returns>nick!user@host, OR nick@host, OR nick</returns>
        public override string ToString()
        {

            string endResult = string.Empty;

            if (this.nickname != null)
                endResult = this.nickname;

            if (this.username != null)
            {
                endResult += "!" + this.username;
            }
            if (this.hostname != null)
            {
                endResult += "@" + this.hostname;
            }

            return endResult;
        }

        /// <summary>
        /// Gets or sets the access level.
        /// </summary>
        /// <value>The access level.</value>
        public UserRights accessLevel
        {
            get
            {
                try
                {
                    if (this._retrievedAccessLevel == false)
                    {
                        DAL.Select q = new DAL.Select("user_accesslevel");
                        q.addWhere(new DAL.WhereConds(true, this.nickname, "LIKE", false, "user_nickname"));
                        q.addWhere(new DAL.WhereConds(true, this.username, "LIKE", false, "user_username"));
                        q.addWhere(new DAL.WhereConds(true, this.hostname, "LIKE", false, "user_hostname"));
                        q.addOrder(new DAL.Select.Order("user_accesslevel", true));
                        q.setFrom("user");

                        string accesslevel = this._db.executeScalarSelect(q) ??
                                             "Normal";

                        UserRights ret =
                            (UserRights)Enum.Parse( typeof( UserRights ), accesslevel );

                        this._accessLevel = ret;
                        this._retrievedAccessLevel = true;
                        return ret;
                    }
                    return this._accessLevel;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }

                return UserRights.Normal;
            }
            set { throw new NotImplementedException(); }
        }

        public enum UserRights
        {
            Developer = 3,
            Superuser = 2,
            Advanced = 1,
            Normal = 0,
            Semiignored = -1,
            Ignored = -2
        }
    }
}