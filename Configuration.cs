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
            return dbal.proc_HMB_GET_LOCAL_OPTION( optionName, channel );
        }

        private string retrieveOptionFromDatabase( string optionName )
        {
            string result = "";
            try
            {
                DAL.Select q = new DAL.Select( "configuration_value" );
                q.setFrom( "configuration" );
                q.addLimit( 1, 0 );
                q.addWhere( new DAL.WhereConds( "configuration_name", optionName ) );

                result = dbal.executeScalarSelect( q );
                if( result == null )
                {
                    result = "";
                }
                return result;
            }
            catch( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex );
            }
            return null;
        }

        public void setGlobalOption( string optionName , string newValue )
        {
            Dictionary<string, string> vals = new Dictionary<string, string>( );
            vals.Add( "configuration_value", newValue );
            dbal.Update( "configuration", vals, 1, new DAL.WhereConds( "configuration_name", optionName ) );
        }
        public void setLocalOption( string optionName, string channel, string newValue )
        {
            // convert channel to ID
            DAL.Select q = new DAL.Select( "channel_id" );
            q.setFrom( "channel" );
            q.addWhere( new DAL.WhereConds( "channel_name", channel ) );

            string channelId = dbal.executeScalarSelect( q );
            q = new DAL.Select( "configuration_id" );
            q.setFrom( "configuration" );
            q.addWhere( new DAL.WhereConds( "configuration_name", optionName ) );

            string configId = dbal.executeScalarSelect( q );

            // does setting exist in local table?
            //  INNER JOIN `channel` ON `channel_id` = `cc_channel` WHERE `channel_name` = '##helpmebot' AND `configuration_name` = 'silence'

            q = new DAL.Select( "COUNT(*)" );
            q.setFrom( "channelconfig" );
            q.addWhere( new DAL.WhereConds( "cc_channel", channelId ) );
            q.addWhere( new DAL.WhereConds( "cc_config", configId ) );
            string count = dbal.executeScalarSelect( q );

            if( count == "1" )
            {
                //yes: update
                Dictionary<string, string> vals = new Dictionary<string, string>( );
                vals.Add( "cc_value", newValue );
                dbal.Update( "channelconfig", vals, 1, new DAL.WhereConds( "cc_channel", channelId ), new DAL.WhereConds( "cc_config", configId ) );
            }
            else
            {
                // no: insert
                dbal.Insert( "channelconfig", channelId, configId, newValue );
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
            dbal.Delete( "channelconfig", 1, new DAL.WhereConds( "cc_config", getOptionId( optionName ) ), new DAL.WhereConds( "cc_channel", getChannelId( target ) ) );
        }

        private string getOptionId( string optionName )
        {
            DAL.Select q = new DAL.Select( "configuration_id" );
            q.setFrom( "configuration" );
            q.addWhere( new DAL.WhereConds( "configuration_name", optionName ) );

            return dbal.executeScalarSelect( q );
        }

        public string getChannelId( string channel )
        {
            DAL.Select q = new DAL.Select( "channel_id" );
            q.setFrom( "channel" );
            q.addWhere( new DAL.WhereConds( "channel_name", channel ) );

            return dbal.executeScalarSelect( q );
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
            //"SELECT m.`message_text` FROM message m WHERE m.`message_name` = '"+messageName+"';" );

            DAL.Select q = new DAL.Select( "message_text" );
            q.setFrom( "message" );
            q.addWhere( new DAL.WhereConds( "message_name", messageName ) );
            
            ArrayList resultset = dbal.executeSelect( q );

            ArrayList al = new ArrayList( );

            foreach( object[] item in resultset )
            {
                al.Add( (string)( item )[ 0 ] );
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
            dbal.Insert( "message", "", messageName, messageDescription, messageContent, "1" );
        }
        #endregion
    }
}
