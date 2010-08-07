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
    internal class AccessLog
    {
        private static AccessLog _instance;

        public static AccessLog instance()
        {
            return _instance ?? ( _instance = new AccessLog( ) );
        }

        protected AccessLog()
        {
        }

        public void save(AccessLogEntry logEntry)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            DAL.singleton().insert("accesslog", "", logEntry.alUser.ToString(), logEntry.alUser.accessLevel.ToString(),
                                   logEntry.alReqaccesslevel.ToString(), "", logEntry.alClass.ToString(),
                                   (logEntry.alAllowed ? "1" : "0"));
        }

        public struct AccessLogEntry
        {
            public AccessLogEntry(User source, Type command, bool success)
            {
                this._alId = 0;
                this._alDate = new DateTime(0);
                this._alUser = source;
                this._alClass = command;
                this._alAllowed = success;
                this._alReqaccesslevel = ((GenericCommand) Activator.CreateInstance(this._alClass)).accessLevel;
            }

            private readonly int _alId;
            private readonly User _alUser;
            private readonly User.UserRights _alReqaccesslevel;
            private readonly Type _alClass;
            private readonly DateTime _alDate;
            private readonly bool _alAllowed;

            public int alId
            {
                get { return this._alId; }
            }

            public User alUser
            {
                get { return this._alUser; }
            }

            public User.UserRights alReqaccesslevel
            {
                get { return this._alReqaccesslevel; }
            }

            public Type alClass
            {
                get { return this._alClass; }
            }

            public DateTime alDate
            {
                get { return this._alDate; }
            }

            public bool alAllowed
            {
                get { return this._alAllowed; }
            }
        }

        public bool doFloodCheck(User source)
        {
            //TODO: Implement
            return false;
        }
    }
}