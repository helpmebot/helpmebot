using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.IO;
namespace helpmebot6
{
    public class DAL
    {
        public static DAL singleton;

        string _mySqlServer, _mySqlUsername, _mySqlPassword, _mySqlSchema;
        uint _mySqlPort;

        MySqlConnection _connection;

        public DAL( string Host, uint Port, string Username, string Password, string Schema )
        {
            _mySqlPort = Port;
            _mySqlPassword = Password;
            _mySqlSchema = Schema;
            _mySqlServer = Host;
            _mySqlUsername = Username;
        }

        public void Connect( )
        {
            MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder( );
            csb.Database = _mySqlSchema;
            csb.Password = _mySqlPassword;
            csb.Server = _mySqlServer;
            csb.UserID = _mySqlUsername;
            csb.Port = _mySqlPort;

            _connection = new MySqlConnection( csb.ConnectionString);
            _connection.Open( );
        }

        public void ExecuteNonQuery( string query )
        {
            try
            {
                MySqlTransaction transact = _connection.BeginTransaction( System.Data.IsolationLevel.RepeatableRead );
                MySqlCommand cmd = new MySqlCommand( query, _connection );//, transact);
                cmd.ExecuteNonQuery( );
                transact.Commit( );
            }
            catch ( MySqlException ex )
            {
                GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod());
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod());
            }
        }

        public string ExecuteScalarQuery( string query )
        {
            object result = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand( query, _connection );
                
                result= cmd.ExecuteScalar( );
            }
            catch ( MySqlException ex )
            {
                GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod());
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod());
            }
            string ret = "";

            if ( result == null )
                ret = "";
            else
                ret = result.ToString( );

            return ret;
        }

        public MySqlDataReader ExecuteReaderQuery( string query )
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand( query );
                cmd.Connection = _connection;
                MySqlDataReader result = cmd.ExecuteReader( );
                return result;
            }
            catch ( MySqlException ex )
            {
                GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod());
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod());
            }

            return null;
        }


    }
}
