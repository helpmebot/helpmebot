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

        public string this[ string globalOption ]
        {
            get
            {
                return retrieveGlobalStringOption( globalOption );
            }
            set
            {
                setGlobalOption( globalOption, value );
            }
        }

        public string retrieveGlobalStringOption( string optionName )
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

                if( optionValue2 != string.Empty )
                {
                    ConfigurationSetting cachedSetting = new ConfigurationSetting( optionName , optionValue2 );
                    configurationCache.Add( cachedSetting );
                }
                return optionValue2;
            
        }
        public uint retrieveGlobalUintOption( string optionName )
        {
            string optionValue = retrieveGlobalStringOption( optionName );
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

        public string retrieveLocalStringOption (string optionName, string channel)
        {
            string qry = "SELECT `cc_value` AS 'value' FROM `channelconfig` INNER JOIN `configuration` ON `cc_config` = " +
            "`configuration_id` INNER JOIN `channel` ON `channel_id` = `cc_channel` WHERE `channel_name` = '" + channel + "' " +
            "AND `configuration_name` = '" + optionName + "' UNION SELECT `configuration_value` AS 'value' FROM " +
            "`configuration` WHERE `configuration_name` = '" + optionName + "' LIMIT 1;";

            string option = DAL.Singleton( ).ExecuteScalarQuery( qry );
            return option;
            
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
                GlobalFunctions.ErrorLog( ex  );
            }
            return null;
        }

        public void setGlobalOption( string optionName , string newValue )
        {
            dbal.ExecuteNonQuery( "UPDATE `configuration` SET `configuration_value` = '" + newValue + "' WHERE `configuration`.`configuration_name` = '" + optionName + "' LIMIT 1;" );
        }
        public void setLocalOption( string optionName , string channel , string newValue )
        {
            // convert channel to ID
            string[] wc = {"channel_name = '" + channel + "'"};
            string channelId = dbal.Select( "channel_id" , "channel" , null , wc , null , null , null , 1 , 0 );
            string[ ] wc2 = {"configuration_name = '" + optionName + "'" };
            string configId = dbal.Select( "configuration_id" , "configuration" , null , wc2 , null , null , null , 1 , 0 );

            // does setting exist in local table?
           //  INNER JOIN `channel` ON `channel_id` = `cc_channel` WHERE `channel_name` = '##helpmebot' AND `configuration_name` = 'silence'

            string[ ] wc3 = { "`cc_channel` = '" + channelId + "'" , "`cc_config` = '" + configId + "'" };
            string count = dbal.Select( "COUNT(*)" , "channelconfig" , null , wc3 , null , null , null , 1 , 0 );

            if( count == "1" )
            {
                //yes: update
                string qry = "UPDATE `channelconfig` SET `cc_value` = '" + newValue + "' WHERE `channelconfig`.`cc_channel` =" + channelId + " AND `channelconfig`.`cc_config` =" + configId + " LIMIT 1 ;";
                dbal.ExecuteNonQuery( qry );
            }
            else
            {
                // no: insert
                string qry = "INSERT INTO `channelconfig` (`cc_channel`, `cc_config`, `cc_value`) VALUES ('" + channelId + "', '" + configId + "', '" + newValue + "');";
                dbal.ExecuteNonQuery( qry );
            }
        }

        public void setOption(  string optionName , string target , string newValue )
        {
            if( target == "global" )
            {
                setGlobalOption( optionName , newValue );
            }
            else
            {
                setLocalOption( optionName , target , newValue );
            }
        }

        public void deleteLocalOption( string optionName , string target )
        {
            DAL.Singleton( ).ExecuteNonQuery( "DELETE FROM channelconfig WHERE cc_config = " + getOptionId( optionName ) + " AND cc_channel = " + getChannelId( target ) + " LIMIT 1;" );
        }

        private string getOptionId( string optionName )
        {
            //SELECT c.`configuration_id` FROM configuration c
            //WHERE c.`configuration_name` = "ircNetwork";

            string[ ] wC = { "c.`configuration_name` = '" + optionName + "'" };
            return DAL.Singleton( ).Select( "c.`configuration_id`" , "configuration c" , null , wC, null , null , null , 1 , 0 );
        }

        public string getChannelId( string channel )
        {
            //SELECT c.`channel_id` FROM channel c
            //WHERE c.`channel_name` = "##stwalkerster";

            string[ ] wC = { "c.`channel_name` = '"+channel+"'" };
            return DAL.Singleton( ).Select( "c.`channel_id`" , "channel c" , null , wC , null , null , null , 1 , 0 );
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

        #region messaging

        private ArrayList getMessages( string messageName )
        {
            MySql.Data.MySqlClient.MySqlDataReader dr =  dbal.ExecuteReaderQuery( "SELECT m.`message_text` FROM message m WHERE m.`message_name` = '"+messageName+"';" );

            System.Collections.ArrayList al = new System.Collections.ArrayList( );

            if( dr != null )
            {
                while( dr.Read( ) )
                {
                    al.Add( dr.GetString( 0 ) );
                }
                dr.Close( );
            }
            else
            {
                GlobalFunctions.ErrorLog( new System.IO.InvalidDataException( )  );
            }
            return al;
        }

        //returns a random message chosen from the list of possible message names
        private string chooseRandomMessage( string messageName )
        {
            Random rnd = new Random( );
            ArrayList al = getMessages( messageName );
            if( al.Count == 0 )
            {
                Helpmebot6.irc.IrcPrivmsg( Helpmebot6.debugChannel , "***ERROR*** Message '" + messageName + "' not found in message table" );
                return "";
            }
            return al[ rnd.Next( 0, al.Count ) ].ToString( );
        }

        private string parseMessage( string messageFormat, string[ ] args )
        {
            return String.Format( messageFormat, args );
        }
        public string GetMessage( string messageName )
        {
            return chooseRandomMessage( messageName );
        }
        public string GetMessage( string messageName, params string[ ] args )
        {
            return parseMessage( chooseRandomMessage( messageName ), args );
        }

        public string GetMessage( string messageName , string defaultMessageName )
        {
            string msg = GetMessage( messageName );
            if( msg == string.Empty )
            {
                msg = GetMessage( defaultMessageName );
                SaveMessage( messageName , "" , msg );
            }
            msg = GetMessage( messageName );
            return msg;
        }
        public string GetMessage( string messageName , string defaultMessageName, params string[ ] args )
        {
            string msg = GetMessage( messageName ,args);
            if( msg == string.Empty )
            {
                msg = GetMessage( defaultMessageName );
                SaveMessage( messageName , "" , msg );
            }
            msg = GetMessage( messageName, args );
            return msg;
        }

        public void SaveMessage( string messageName , string messageDescription , string messageContent )
        {
            DAL.Singleton( ).ExecuteNonQuery( "INSERT INTO `message` VALUES ( NULL, \"" + messageName + "\", \"" + messageDescription + "\", \"" + messageContent + "\" , 1);" );
        }
        #endregion
    }
}
