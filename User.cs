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

#endregion

namespace helpmebot6
{
    public class User
    {
        private readonly DAL _db;

        private UserRights _accessLevel;
        private bool _retrievedAccessLevel;

        public User()
        {
            this._db = DAL.singleton();
        }

        public string nickname { get; set; }

        public string username { get; set; }

        public string hostname { get; set; }

        public uint network { get; private set; }

        public static User newFromString(string source)
        {
            return newFromString(source, 0);
        }

        public static User newFromString(string source, uint network)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

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
                GlobalFunctions.errorLog(ex);
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

        /// <summary>
        ///   Recompiles the source string
        /// </summary>
        /// <returns>nick!user@host, OR nick@host, OR nick</returns>
        public override string ToString()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string endResult = this.nickname;

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

                        UserRights ret;

                        switch (accesslevel)
                        {
                            case "Developer":
                                ret = UserRights.Developer;
                                break;
                            case "Superuser":
                                ret = UserRights.Superuser;
                                break;
                            case "Advanced":
                                ret = UserRights.Advanced;
                                break;
                            case "Normal":
                                ret = UserRights.Normal;
                                break;
                            case "Semi-ignored":
                                ret = UserRights.Semiignored;
                                break;
                            case "Ignored":
                                ret = UserRights.Ignored;
                                break;
                            default:
                                ret = UserRights.Normal;
                                break;
                        }

                        _accessLevel = ret;
                        this._retrievedAccessLevel = true;
                        return ret;
                    }
                    return this._accessLevel;
                }
                catch (Exception ex)
                {
                    GlobalFunctions.errorLog(ex);
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