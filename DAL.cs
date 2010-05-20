/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.IO;
using System.Collections;
using System.Data;

namespace helpmebot6
{
    public class DAL
    {
        static DAL _singleton;

        string _mySqlServer, _mySqlUsername, _mySqlPassword, _mySqlSchema;
        uint _mySqlPort;

        MySqlConnection _connection;

        public static DAL Singleton( )
        {
            return _singleton;
        }
        public static DAL Singleton( string Host, uint Port, string Username, string Password, string Schema )
        {
            if( _singleton == null )
                _singleton = new DAL( Host, Port, Username, Password, Schema );
            return _singleton;
        }

        protected DAL( string Host, uint Port, string Username, string Password, string Schema )
        {
            _mySqlPort = Port;
            _mySqlPassword = Password;
            _mySqlSchema = Schema;
            _mySqlServer = Host;
            _mySqlUsername = Username;
        }

        public bool Connect( )
        {
            try
            {
                Logger.Instance( ).addToLog( "Opening database connection...", Logger.LogTypes.DAL );
                MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder( );
                csb.Database = _mySqlSchema;
                csb.Password = _mySqlPassword;
                csb.Server = _mySqlServer;
                csb.UserID = _mySqlUsername;
                csb.Port = _mySqlPort;

                _connection = new MySqlConnection( csb.ConnectionString );
                _connection.Open( );
                return true;
            }
            catch( MySqlException ex )
            {
                GlobalFunctions.ErrorLog( ex );
                return false;
            }
        }

        #region internals
        private void ExecuteNonQuery( ref MySqlCommand cmd )
        {
            Logger.Instance( ).addToLog( "Locking access to DAL...", Logger.LogTypes.DALLOCK );
            lock( this )
            {
                Logger.Instance( ).addToLog( "Executing (non)query: " + cmd.CommandText, Logger.LogTypes.DAL );
                try
                {

                    runConnectionTest( );
                    //MySqlTransaction transact = _connection.BeginTransaction( System.Data.IsolationLevel.RepeatableRead );
                    cmd.Connection = _connection;
                    cmd.ExecuteNonQuery( );
                    //transact.Commit( );
                }
                catch( MySqlException ex )
                {
                    GlobalFunctions.ErrorLog( ex );
                }
                catch( Exception ex )
                {
                    GlobalFunctions.ErrorLog( ex );
                }
                Logger.Instance( ).addToLog( "Done executing (non)query: " + cmd.CommandText, Logger.LogTypes.DAL );
            }
            Logger.Instance( ).addToLog( "DAL Lock released.", Logger.LogTypes.DALLOCK );
        }

        private string ExecuteScalarQuery( string query )
        {
            string ret = "";
            Logger.Instance( ).addToLog( "Locking access to DAL...", Logger.LogTypes.DALLOCK );
            lock( this )
            {
                Logger.Instance( ).addToLog( "Executing (scalar)query: " + query, Logger.LogTypes.DAL );

                object result = null;
                try
                {
                    runConnectionTest( );

                    MySqlCommand cmd = new MySqlCommand( query, _connection );

                    result = cmd.ExecuteScalar( );
                }
                catch( MySqlException ex )
                {
                    GlobalFunctions.ErrorLog( ex );
                }
                catch( Exception ex )
                {
                    GlobalFunctions.ErrorLog( ex );
                }


                if( result == null )
                {
                    Logger.Instance( ).addToLog( "Problem executing (scalar)query: " + query, Logger.LogTypes.DAL );
                    ret = "";
                }
                else
                {
                    ret = result.ToString( );
                    Logger.Instance( ).addToLog( "Done executing (scalar)query: " + query, Logger.LogTypes.DAL );
                }
            }
            Logger.Instance( ).addToLog( "DAL Lock released.", Logger.LogTypes.DALLOCK );
            return ret;
        }

        private MySqlDataReader ExecuteReaderQuery( string query )
        {
            MySqlDataReader result = null;

            Logger.Instance( ).addToLog( "Locking access to DAL...", Logger.LogTypes.DALLOCK );
            lock( this )
            {
                Logger.Instance( ).addToLog( "Executing (reader)query: " + query, Logger.LogTypes.DAL );

                try
                {
                    runConnectionTest( );

                    MySqlCommand cmd = new MySqlCommand( query );
                    cmd.Connection = _connection;
                    result = cmd.ExecuteReader( );
                    Logger.Instance( ).addToLog( "Done executing (reader)query: " + query, Logger.LogTypes.DAL );

                    return result;
                }
                catch( Exception ex )
                {
                    Logger.Instance( ).addToLog( "Problem executing (reader)query: " + query, Logger.LogTypes.DAL );
                    GlobalFunctions.ErrorLog( ex );
                }
            }
            Logger.Instance( ).addToLog( "DAL Lock released.", Logger.LogTypes.DALLOCK );
            return result;
        }
        #endregion

        public long Insert( string table, params string[ ] values )
        {
            string query = "INSERT INTO `" + sanitise( table ) + "` VALUES (";
            foreach( string item in values )
            {
                if( item != string.Empty )
                {
                    query += " \"" + sanitise( item ) + "\",";
                }
                else
                {
                    query += "null,";
                }
            }

            query = query.TrimEnd( ',' );
            query += " );";

            MySqlCommand cmd = new MySqlCommand( query );
            ExecuteNonQuery( ref cmd );
            return cmd.LastInsertedId;
        }

        public void Delete( string table, int limit, params WhereConds[ ] conditions )
        {
            string query = "DELETE FROM `" + sanitise( table ) + "`";
            for( int i = 0; i < conditions.Length; i++ )
            {
                if( i == 0 )
                    query += " WHERE ";
                else
                    query += " AND ";

                query += conditions[ i ].ToString( );
            }

            if( limit > 0 )
                query += " LIMIT " + limit.ToString( );

            query += ";";
            MySqlCommand deleteCommand = new MySqlCommand( query );
            ExecuteNonQuery( ref deleteCommand );
        }

        public void Update( string table, Dictionary<string, string> items, int limit, params WhereConds[ ] conditions )
        {
            if( items.Count < 1 )
                return;

            string query = "UPDATE `" + sanitise( table ) + "` SET ";

            foreach( KeyValuePair<string, string> col in items )
            {
                query += "`" + sanitise( col.Key ) + "` = \"" + sanitise( col.Value ) + "\", ";
            }

            query = query.TrimEnd( ',' );

            for( int i = 0; i < conditions.Length; i++ )
            {
                if( i == 0 )
                    query += " WHERE ";
                else
                    query += " AND ";

                query += conditions[ i ].ToString( );
            }

            if( limit > 0 )
                query += " LIMIT " + limit.ToString( );

            query += ";";

            MySqlCommand updateCommand = new MySqlCommand( query );
            ExecuteNonQuery( ref updateCommand );
        }

        public ArrayList executeSelect( Select query )
        {
            MySqlDataReader dr = ExecuteReaderQuery( query.ToString() );

            ArrayList resultSet = new ArrayList( );
            if( dr != null )
            {
                object[ ] row = new object[ dr.FieldCount ];
                while( dr.Read( ) )
                {
                    row = new object[ dr.FieldCount ];
                    dr.GetValues( row );
                    resultSet.Add( row );
                }
                dr.Close( );
            }
            return resultSet;
        }
        public string executeScalarSelect( Select query )
        {
            return (string)( ( (object[ ])executeSelect( query )[ 0 ] )[ 0 ] );
        }

        private void runConnectionTest( )
        {
            // ok, first let's assume the connection is dead.
            bool connectionOk = false;

            // first time through, skip the connection attempt
            bool firstTime = true;

            int sleepTime = 1000;

            while( !connectionOk )
            {
                if( !firstTime )
                {
                    Logger.Instance( ).addToLog( "Reconnecting to database....", Logger.LogTypes.ERROR );

                    Connect( );

                    System.Threading.Thread.Sleep( sleepTime );

                    sleepTime = (int)( sleepTime * 1.5 ) > int.MaxValue ? sleepTime : (int)( sleepTime * 1.5 );

                }

                connectionOk = _connection.Ping( );

            }
        }

        public void executeProcedure( string name, params string[ ] args )
        {
            MySqlCommand cmd = new MySqlCommand( );
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = name;

            foreach( string item in args )
            {
                cmd.Parameters.Add( new MySqlParameter( item, MySqlDbType.Int16 ) );
            }

            cmd.Connection = _connection;

            runConnectionTest( );

            cmd.ExecuteNonQuery( );
        }

        public void proc_HMB_UPDATE_BINARYSTORE( byte[ ] raw, string desc )
        {
            lock( this )
            {
                runConnectionTest( );

                MySqlCommand cmd = new MySqlCommand( );
                cmd.Connection = _connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "HMB_UPDATE_BINARYSTORE";
                cmd.Parameters.Add( "@raw", MySqlDbType.Blob ).Value = raw;
                cmd.Parameters.Add( "@desc", MySqlDbType.VarChar ).Value = desc;

                cmd.ExecuteNonQuery( );
            }
        }

        public string proc_HMB_GET_LOCAL_OPTION( string option, string channel )
        {
            

            MySqlCommand cmd = new MySqlCommand( );
            cmd.Connection = _connection;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "HMB_GET_LOCAL_OPTION";

            cmd.Parameters.AddWithValue( "@optionName", option );
            cmd.Parameters[ "@optionName" ].Direction = ParameterDirection.Input;

            cmd.Parameters.AddWithValue( "@channel", channel );
            cmd.Parameters[ "@channel" ].Direction = ParameterDirection.Input;

            cmd.Parameters.AddWithValue( "@optionValue", MySqlDbType.VarChar );
            cmd.Parameters[ "@optionValue" ].Direction = ParameterDirection.Output;
            lock( this )
            {
                runConnectionTest( );
                cmd.ExecuteNonQuery( );
            }
            return (string)cmd.Parameters[ "@optionValue" ].Value;

        }


        /// <summary>
        /// Class encapsulating a SELECT statement
        /// </summary>
        public class Select
        {
            private bool shallIEscapeSelects = true;

            private string[ ] fields;
            private string from;
            private LinkedList<Join> joins = new LinkedList<Join>( );
            private LinkedList<WhereConds> wheres;
            private LinkedList<string> groups;
            private LinkedList<Order> orders;
            private LinkedList<WhereConds> havings;
            private int limit;
            private int offset;

            public Select( params string[ ] fields )
            {
                this.fields = fields;
                from = string.Empty;
                limit = offset = 0;
                joins = new LinkedList<Join>( );
                wheres = new LinkedList<WhereConds>( );
                groups = new LinkedList<string>( );
                orders = new LinkedList<Order>( );
                havings = new LinkedList<WhereConds>( );
            }

            public void escapeSelects( bool escape )
            {
                this.shallIEscapeSelects = escape;
            }

            public void setFrom( string from )
            {
                this.from = from;
            }

            public void addJoin( string table, JoinTypes joinType, WhereConds conditions )
            {
                joins.AddLast( new Join( joinType, table, conditions ) );
            }

            public void addWhere( WhereConds conditions )
            {
                wheres.AddLast( conditions );
            }

            public void addGroup( string field )
            {
                groups.AddLast( field );
            }

            public void addOrder( Order order )
            {
                orders.AddLast( order );
            }

            public void addHaving( WhereConds conditions )
            {
                havings.AddLast( conditions );
            }

            public void addLimit( int limit, int offset )
            {
                this.limit = limit;
                this.offset = offset;
            }

            public override string ToString( )
            {
                string query = "SELECT ";
                bool firstField = true;
                foreach( string  f in fields )
                {
                    if( !firstField )
                        query += ", ";

                    string fok = MySqlHelper.EscapeString(f);
                    if( ! shallIEscapeSelects )
                        fok = f;

                    firstField = false;

                    query += fok;
                }

                if( from != string.Empty )
                {
                    query += " FROM " + "`" + MySqlHelper.EscapeString( from ) + "`";
                }

                if( joins.Count != 0 )
                {

                    foreach( Join item in joins )
                    {
                        switch( item.joinType )
                        {
                            case JoinTypes.INNER:
                                query += " INNER JOIN ";
                                break;
                            case JoinTypes.LEFT:
                                query += " LEFT OUTER JOIN ";
                                break;
                            case JoinTypes.RIGHT:
                                query += " RIGHT OUTER JOIN ";
                                break;
                            case JoinTypes.FULLOUTER:
                                query += " FULL OUTER JOIN ";
                                break;
                            default:
                                break;
                        }

                        query += "`" + MySqlHelper.EscapeString(item.table) + "`";

                        query += " ON " + item.joinConditions.ToString( );
                    }
                }

                if( wheres.Count > 0 )
                {
                    query += " WHERE ";

                    bool first = true;

                    foreach( WhereConds w in wheres )
                    {
                        if( !first )
                            query += " AND ";
                        first = false;
                        query += w.ToString( );
                    }

                }
                if( groups.Count != 0 )
                {
                    query += " GROUP BY ";
                    bool first = true;
                    foreach( string group in groups )
                    {
                        if( !first )
                            query += ", ";
                        first = false;
                        query += MySqlHelper.EscapeString(group);
                    }
                }
                if( orders.Count > 0 )
                {
                    query += " ORDER BY ";

                    bool first = true;
                    foreach( Order order in orders )
                    {
                        if( !first )
                            query += ", ";
                        first = false;
                        query += order.ToString( );
                    }

                }
                if( havings.Count > 0 )
                {
                    query += " HAVING ";

                    bool first = true;

                    foreach( WhereConds w in havings )
                    {
                        if( !first )
                            query += " AND ";
                        first = false;
                        query += w.ToString( );
                    }

                }

                if( limit != 0 )
                    query += " LIMIT " + limit;

                if( offset != 0 )
                    query += " OFFSET " + offset;

                query += ";";
                return query;
            }

            public struct Order
            {
                public Order( string column, bool asc )
                {
                    this.column = column;
                    this.asc = asc;
                    escape = true;
                }

                public Order( string column, bool asc, bool escape )
                {
                    this.column = column;
                    this.asc = asc;
                    this.escape = escape;
                }

                private string column;
                private bool asc;
                private bool escape;

                public override string ToString( )
                {
                    return "`" + ( escape ? MySqlHelper.EscapeString( column ) : column ) + "` " + ( asc ? "ASC" : "DESC" );
                }
            }

            private struct Join
            {
                public JoinTypes joinType;
                public string table;
                public WhereConds joinConditions;
                public Join( JoinTypes type, string table, WhereConds conditions)
                {
                    this.joinType = type;
                    this.table = table;
                    this.joinConditions = conditions;
                }
            }

            public enum JoinTypes
            {
                INNER,
                LEFT,
                RIGHT,
                FULLOUTER
            }
        }
        public struct WhereConds
        {
            bool quoteA, quoteB;
            string a, b, comparer;
            public WhereConds( bool aNeedsQuoting, string a, string comparer, bool bNeedsQuoting, string b )
            {
                this.quoteA = aNeedsQuoting;
                this.quoteB = bNeedsQuoting;
                this.a = a;
                this.b = b;
                this.comparer = comparer;
            }
            public WhereConds( string column, string value )
            {
                this.quoteA = false;
                this.quoteB = true;
                this.a = column;
                this.b = value;
                this.comparer = "=";
            }
            public WhereConds( string column, int value )
            {
                this.quoteA = false;
                this.quoteB = true;
                this.a = column;
                this.b = value.ToString();
                this.comparer = "=";
            }
            public override string ToString( )
            {
                string actualA = ( quoteA ? "\"" : "" ) + MySqlHelper.EscapeString( a ) + ( quoteA ? "\"" : "" );
                string actualB = ( quoteB ? "\"" : "" ) + MySqlHelper.EscapeString( b ) + ( quoteB ? "\"" : "" );
                string actualComp = MySqlHelper.EscapeString( comparer );
                return actualA + " " + actualComp + " " + actualB;
            }
        }

        private string sanitise( string rawData )
        {
            return MySqlHelper.EscapeString( rawData );
        }

    }
}