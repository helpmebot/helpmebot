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
namespace helpmebot6
{
    public class DAL
    {
        static DAL _singleton;

        string _mySqlServer, _mySqlUsername, _mySqlPassword, _mySqlSchema;
        uint _mySqlPort;

        MySqlConnection _connection;

        public static DAL Singleton()
        {               
            return _singleton;
        }
        public static DAL Singleton(string Host, uint Port, string Username, string Password, string Schema)
        {
            if (_singleton == null)
                _singleton = new DAL(Host, Port, Username, Password, Schema);
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
                Logger.Instance( ).addToLog( "Opening database connection..." , Logger.LogTypes.DAL );
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
                GlobalFunctions.ErrorLog( ex  );
                return false;
            }
        }

        public void ExecuteNonQuery( string query )
        {
            ExecuteNonQuery( new MySqlCommand( query, _connection ) );
        }

        public void ExecuteNonQuery( MySqlCommand cmd )
        {
            Logger.Instance( ).addToLog( "Locking access to DAL...", Logger.LogTypes.DALLOCK );
            lock( this )
            {
                Logger.Instance( ).addToLog( "Executing (non)query: " + cmd.CommandText , Logger.LogTypes.DAL );
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
                Logger.Instance( ).addToLog( "Done executing (non)query: " + cmd.CommandText , Logger.LogTypes.DAL );
            }
            Logger.Instance( ).addToLog( "DAL Lock released.", Logger.LogTypes.DALLOCK );
        }

        public string ExecuteScalarQuery( string query )
        {
            string ret = "";
            Logger.Instance( ).addToLog( "Locking access to DAL...", Logger.LogTypes.DALLOCK );
            lock( this )
            {
                Logger.Instance( ).addToLog( "Executing (scalar)query: " + query , Logger.LogTypes.DAL );

                object result = null;
                try
                {
                    runConnectionTest( );

                    MySqlCommand cmd = new MySqlCommand( query , _connection );

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
                    Logger.Instance( ).addToLog( "Problem executing (scalar)query: " + query , Logger.LogTypes.DAL );
                    ret = "";
                }
                else
                {
                    ret = result.ToString( );
                    Logger.Instance( ).addToLog( "Done executing (scalar)query: " + query , Logger.LogTypes.DAL );
                } 
            }
            Logger.Instance( ).addToLog( "DAL Lock released.", Logger.LogTypes.DALLOCK );
            return ret;
        }

        public MySqlDataReader ExecuteReaderQuery( string query )
        {
            MySqlDataReader result = null;
            
            Logger.Instance( ).addToLog( "Locking access to DAL..." , Logger.LogTypes.DALLOCK );
            lock( this )
            {
                Logger.Instance( ).addToLog( "Executing (reader)query: " + query , Logger.LogTypes.DAL );

                try
                {
                    runConnectionTest( );

                    MySqlCommand cmd = new MySqlCommand( query );
                    cmd.Connection = _connection;
                    result = cmd.ExecuteReader( );
                    Logger.Instance( ).addToLog( "Done executing (reader)query: " + query , Logger.LogTypes.DAL );

                    return result;
                }
                catch( Exception ex )
                {
                    Logger.Instance( ).addToLog( "Problem executing (reader)query: " + query , Logger.LogTypes.DAL );
                    GlobalFunctions.ErrorLog( ex );
                }
            }
            Logger.Instance( ).addToLog( "DAL Lock released.", Logger.LogTypes.DALLOCK );
            return result;
        }

        public enum joinTypes
        {
            INNER,
            LEFT,
            RIGHT,
            FULLOUTER
        }

        public struct join
        {
            public joinTypes joinType;
            public string table;
            public string joinConditions;
        }

        public struct order
        {
            public string column;
            public bool asc;
        }

        public string Select( string select , string from , join[ ] joinConds , string[ ] where , string[ ] groupby , order[ ] orderby , string[ ] having , int limit , int offset )
        {

            try
            {
                string[ ] selectArray = { select };
                string query = buildSelect( selectArray , from , joinConds , where , groupby , orderby , having , limit , offset );
                Logger.Instance( ).addToLog( "Running SELECT query: " + query , Logger.LogTypes.DAL );

                string result = ExecuteScalarQuery( query );
                Logger.Instance( ).addToLog( "Done SELECT query: " + query , Logger.LogTypes.DAL );

                return result;
            }
            catch( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex  );
                return "";
            }
        }

        private string buildSelect( string[] select , string from , join[ ] joinConds , string[ ] where , string[ ] groupby , order[ ] orderby , string[ ] having , int limit , int offset )
        {
            string query = "SELECT " + string.Join(", ",select) + " FROM " + from;

            if( joinConds != null )
            {

                foreach( join item in joinConds )
                {
                    switch( item.joinType )
                    {
                        case joinTypes.INNER:
                            query += " INNER JOIN ";
                            break;
                        case joinTypes.LEFT:
                            query += " LEFT OUTER JOIN ";
                            break;
                        case joinTypes.RIGHT:
                            query += " RIGHT OUTER JOIN ";
                            break;
                        case joinTypes.FULLOUTER:
                            query += " FULL OUTER JOIN ";
                            break;
                        default:
                            break;
                    }

                    query += item.table;

                    if( item.joinConditions != "" )
                    {
                        query += " ON " + item.joinConditions;
                    }
                }
            }

            if( where != null )
            {
                if( where.Length > 0 )
                {
                    query += " WHERE ";

                    for( int i = 0 ; i < where.Length ; i++ )
                    {
                        if( i != 0 )
                            query += " AND ";

                        query += where[ i ];
                    }
                }
            }
            if( groupby != null )
            {
                if( groupby.Length > 0 )
                {
                    query += " GROUP BY ";

                    for( int i = 0 ; i < groupby.Length ; i++ )
                    {
                        if( i != 0 )
                            query += ", ";

                        query += groupby[ i ];
                    }
                }
            }
            if( orderby != null )
            {
                if( orderby.Length > 0 )
                {
                    query += " ORDER BY ";

                    for( int i = 0 ; i < orderby.Length ; i++ )
                    {
                        if( i != 0 )
                            query += ", ";

                        query += orderby[ i ].column + ( orderby[ i ].asc ? " ASC" : " DESC" );

                    }
                }
            }
            if( having != null )
            {
                if( having.Length > 0 )
                {
                    query += " HAVING ";

                    for( int i = 0 ; i < having.Length ; i++ )
                    {
                        if( i != 0 )
                            query += ", ";

                        query += having[ i ];
                    }
                }
            }

            if( limit != 0 )
                query += " LIMIT " + limit;

            if( offset != 0 )
                query += " OFFSET " + offset;

            query += ";";
            return query;
        }
        public ArrayList Select( string[] select , string from , join[ ] joinConds , string[ ] where , string[ ] groupby , order[ ] orderby , string[ ] having , int limit , int offset )
        {
            try
            {
                string query = buildSelect( select , from , joinConds , where , groupby , orderby , having , limit , offset );
                Logger.Instance( ).addToLog( "Running SELECT query: " + query , Logger.LogTypes.DAL );

                MySqlDataReader dr = ExecuteReaderQuery( query );

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
                Logger.Instance( ).addToLog( "Done SELECT query: " + query , Logger.LogTypes.DAL );

                return resultSet;
                
            }
            catch( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex );
                return null;
            }
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

                    sleepTime = (int)(sleepTime * 1.5) > int.MaxValue ? sleepTime : (int)(sleepTime * 1.5);

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
    }
}
