using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.RegularExpressions;
 
namespace helpmebot6.Monitoring
{
    class NewbieWelcomer
    {
        private static NewbieWelcomer _instance;
        protected NewbieWelcomer( )
        {
            DAL.Select q = new DAL.Select( "bin_blob" );
            q.setFrom("binary_store");
            q.addWhere( new DAL.WhereConds( "bin_desc", "newbie_hostnames" ) );
            System.Collections.ArrayList result = DAL.Singleton( ).executeSelect( q );
 
           byte[ ] list =  ( (byte[ ])( ( (object[ ])( result[ 0 ] ) )[ 0 ] ) );
            

           BinaryFormatter bf = new BinaryFormatter( );
           try
           {
               hostNames = (SerializableArrayList)bf.Deserialize( new MemoryStream( list ) );
           }
           catch( System.Runtime.Serialization.SerializationException ex )
           {
               GlobalFunctions.ErrorLog( ex );
               hostNames = new SerializableArrayList( );
           }
            
        }
        public static NewbieWelcomer Instance( )
        {
            if( _instance == null )
                _instance = new NewbieWelcomer( );
            return _instance;
        }

        SerializableArrayList hostNames; 

        public void execute( User source, string channel )
        {
            if( Configuration.Singleton( ).retrieveLocalStringOption( "welcomeNewbie", channel ) == "true" )
            {
                bool match = false;
                foreach( object item in hostNames )
                {
                    string pattern = (string)item;
                    Regex rX = new Regex( pattern );
                    if( rX.IsMatch( source.Hostname ) )
                    {
                        match = true;
                        break;
                    }
                }

                if( match )
                {
                    string[ ] cmdArgs = { source.Nickname, channel };
                    Helpmebot6.irc.IrcPrivmsg( channel, Configuration.Singleton( ).GetMessage( "welcomeMessage", cmdArgs ) );
                }
            }
        }

        public void addHost( string host )
        {
            hostNames.Add( host );

            saveHostnames( );
        }

        public void delHost( string host )
        {
            hostNames.Remove( host );

            saveHostnames( );
        }

        public string[ ] getHosts( )
        {
            string[] list = new string[hostNames.Count];
            hostNames.CopyTo( list );
            return list;
        }

        private void saveHostnames( )
        {
            BinaryFormatter bf = new BinaryFormatter( );
            MemoryStream ms = new MemoryStream( );
            bf.Serialize( ms, hostNames );

            byte[ ] buf = ms.GetBuffer( );

            DAL.Singleton( ).proc_HMB_UPDATE_BINARYSTORE( buf, "newbie_hostnames" );
        }
    }

}
