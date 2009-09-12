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
using System.IO;
using System.Collections;

namespace helpmebot6
{
    public class Configuration
    {
        DAL dbal = DAL.Singleton();

        private static Configuration _singleton;
        public static Configuration Singleton( )
        {
            if( _singleton == null )
                _singleton = new Configuration( );
            return _singleton;
        }
        protected Configuration(  )
        {
            configurationCache = new System.Collections.ArrayList( );
        }


        System.Collections.ArrayList configurationCache;

        public string retrieveStringOption( string optionName )
        {
          
            foreach ( ConfigurationSetting s in configurationCache )
            {
                if ( s.Name == optionName )
                { // option found, deal with option
          
                    if ( s.isValid( ) == true )
                    {//option cache is still valid
                        return s.Value;
                    }
                    else
                    {//option cache is not valid
                        // fetch new item from database
                        string optionValue1 = retrieveOptionFromDatabase( optionName );

                        s.Value = optionValue1;
                        return s.Value;
                    }
                }

            }

          
           // option not found, add entry to cache
                string optionValue2 = retrieveOptionFromDatabase( optionName );


                ConfigurationSetting cachedSetting = new ConfigurationSetting( optionName, optionValue2 );
                configurationCache.Add( cachedSetting );

                return optionValue2;
            
        }
        public uint retrieveUintOption( string optionName )
        {
            string optionValue = retrieveStringOption( optionName );
            uint value;
            try
            {
                value = uint.Parse( optionValue );
            }
            catch ( Exception )
            {
                return 0;
            }
            return value;
        }

        private string retrieveOptionFromDatabase( string optionName )
        {
            string result = "";
            try
            {
                string[] whereclause = {"configuration_name = \""+optionName+"\""};
                result = dbal.Select( "configuration_value" , "configuration" , new DAL.join[ 0 ] , whereclause , new string[ 0 ] , new DAL.order[ 0 ] , new string[ 0 ] , 1 , 0 );
                //result = dbal.ExecuteScalarQuery( "SELECT c.`configuration_value` FROM configuration c WHERE c.`configuration_name` = \"" + optionName + "\" LIMIT 1;" );
                if ( result == null )
                {
                    result = "";
                }
                return result;
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod());
            }
            return null;
        }

        public static void readHmbotConfigFile( string filename, 
                ref string mySqlServerHostname, ref string mySqlUsername, 
                ref string mySqlPassword, ref uint mySqlServerPort, 
                ref string mySqlSchema )
        {
            StreamReader settingsreader = new StreamReader( filename );
            mySqlServerHostname = settingsreader.ReadLine( );
            mySqlServerPort = uint.Parse( settingsreader.ReadLine( ) );
            mySqlUsername = settingsreader.ReadLine( );
            mySqlPassword = settingsreader.ReadLine( );
            mySqlSchema = settingsreader.ReadLine( );
            settingsreader.Close( );
        }

        private ArrayList getMessages( string messageName )
        {
            MySql.Data.MySqlClient.MySqlDataReader dr =  dbal.ExecuteReaderQuery( "SELECT m.`message_text` FROM message m WHERE m.`message_name` = 'cmdSayHi1';" );

            System.Collections.ArrayList al = new System.Collections.ArrayList( );
            
            while ( dr.Read() )
            {
                al.Add( dr.GetString( 0 ) );                
            }
            dr.Close( );
            return al;
        }

        //returns a random message chosen from the list of possible message names
        private string chooseRandomMessage( string messageName )
        {
            Random rnd = new Random( );
            ArrayList al = getMessages( messageName );
            return al[ rnd.Next( 0, al.Count ) ].ToString( );
        }

        private string parseMessage( string messageFormat, string[ ] args )
        {
            return String.Format( messageFormat, args );
        }
        private string parseMessage( string messageFormat, string arg )
        {
            return String.Format( messageFormat, arg );
        }

        public string GetMessage( string messageName, string arg )
        {
            return parseMessage( chooseRandomMessage( messageName ), arg );
        }

        public string GetMessage( string messageName, string[ ] args )
        {
            return parseMessage( chooseRandomMessage( messageName ), args );
        }
    }
}
