#region Usings

using System;
using System.Reflection;
using helpmebot6.Commands;

#endregion

namespace helpmebot6
{
    public class AccessLog
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
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return false;
        }
    }
}