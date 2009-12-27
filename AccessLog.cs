using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6
{
    public class AccessLog
    {
        private static AccessLog _instance;
        public static AccessLog instance( )
        {
            if( _instance == null )
                _instance = new AccessLog( );
            return _instance;
        }
        protected AccessLog( )
        {
        }

        public void Save( AccessLogEntry logEntry )
        {
            DAL.Singleton( ).ExecuteNonQuery( "INSERT INTO accesslog VALUES (null, '" + logEntry.al_user.ToString( ) + "', '" + logEntry.al_user.AccessLevel.ToString( ) + "', '" + logEntry.al_reqaccesslevel.ToString( ) + "', null, '" + logEntry.al_class.ToString( ) + "', " + ( logEntry.al_allowed ? 1 : 0 ) + ");" );
        }

        public struct AccessLogEntry
        {
            public AccessLogEntry( User source, Type command, bool success )
            {
                _al_id = 0;
                _al_date = new DateTime( 0 );
                _al_user = source;
                _al_class = command;
                _al_allowed = success;
                _al_reqaccesslevel = ( (Commands.GenericCommand)Activator.CreateInstance( _al_class ) ).accessLevel;
            }

            int _al_id;
            User _al_user;
            User.userRights _al_reqaccesslevel;
            Type _al_class;
            DateTime _al_date;
            bool _al_allowed;

            public int al_id
            {
                get
                {
                    return _al_id;
                }
            }

            public User al_user
            {
                get
                {
                    return _al_user;
                }
                private set
                {
                    _al_user = value;
                }
            }

            public User.userRights al_reqaccesslevel
            {
                get
                {
                    return _al_reqaccesslevel;
                }
                private set
                {
                    _al_reqaccesslevel = value;
                }
            }

            public Type al_class
            {
                get
                {
                    return _al_class;
                }
                private set
                {
                    _al_class = value;
                }
            }

            public DateTime al_date
            {
                get
                {
                    return _al_date;
                }
            }

            public bool al_allowed
            {
                get
                {
                    return _al_allowed;
                }
                private set
                {
                    _al_allowed = value;
                }
            }
        }

        public bool doFloodCheck( User source )
        {
            return false;
        }
    }
}