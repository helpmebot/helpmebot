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
using System.Reflection;

namespace helpmebot6
{
    public class GlobalFunctions
    {

        /// <summary>
        /// Searches the array haystack for needle
        /// </summary>
        /// <param name="needle"></param>
        /// <param name="haystack"></param>
        /// <returns>ID of the needle in the haystack, -1 if not in array</returns>
        public static int isInArray( string needle, string[ ] haystack )
        {
            int id = 0;
            foreach ( string straw in haystack )
            {
                if ( needle == straw )
                    return id;
                id++;
            }

            return -1;
        }

        /// <summary>
        /// Searches the array haystack for the first needle who's head matches the needlehead we're looking for
        /// </summary>
        /// <param name="needle"></param>
        /// <param name="haystack"></param>
        /// <returns>ID of the needle in the haystack, -1 if not in array</returns>
        public static int prefixIsInArray( string needlehead, string[ ] haystack )
        {
            int id = 0;
            foreach ( string straw in haystack )
            {
                if( straw.Length >= needlehead.Length )
                {
                    if( needlehead == straw.Substring( 0 , needlehead.Length ) )
                        return id;
                }
                id++;
            }

            return -1;
        }

        /// <summary>
        /// Remove the first item from an array, and return the item
        /// </summary>
        /// <param name="list">The array in question</param>
        /// <returns>The first item from the array</returns>
        public static string popFromFront( ref string[ ] list )
        {
            string firstItem = list[ 0 ];
            list = string.Join( " ", list, 1, list.Length - 1 ).Split( ' ' );
            return firstItem;
        }

        /// <summary>
        /// Log an exception to the log and IRC
        /// </summary>
        /// <param name="ex">The exception thrown</param>
        public static void ErrorLog(Exception ex)
        {
            Logger.Instance().addToLog( ex.ToString( ) + ex.StackTrace , Logger.LogTypes.ERROR);

            System.Diagnostics.StackTrace stack = new System.Diagnostics.StackTrace( );
            MethodBase method = stack.GetFrame( 1 ).GetMethod( );

            if( Helpmebot6.irc != null )
            {
                Helpmebot6.irc.IrcPrivmsg( Helpmebot6.debugChannel , "***ERROR*** in " + method.Name + ": " + ex.Message );
            }
        }

        public static string escape( string str )
        {
            return MySql.Data.MySqlClient.MySqlHelper.EscapeString( str );
        }

        //public static void Log(string message) { Console.WriteLine("# " + message); }

        public static User.userRights commandAccessLevel( )
        {
            string typename = "";
            System.Diagnostics.StackTrace foo = new System.Diagnostics.StackTrace( );
            typename = foo.GetFrame( 1 ).GetMethod( ).DeclaringType.FullName;

            User.userRights accessLevel;
            string[ ] wc = { "typename = \""+typename+"\"" };
            string al = DAL.Singleton( ).Select( "accesslevel" , "command" , null , wc , null , null , null , 1 , 0 );
            switch( al )
            {
                case "Developer":
                    accessLevel = User.userRights.Developer;
                    break;
                case "Superuser":
                    accessLevel = User.userRights.Superuser;
                    break;
                case "Advanced":
                    accessLevel = User.userRights.Advanced;
                    break;
                case "Normal":
                    accessLevel = User.userRights.Normal;
                    break;
                case "Semi-ignored":
                    accessLevel = User.userRights.Semiignored;
                    break;
                case "Ignored":
                    accessLevel = User.userRights.Ignored;
                    break;
                default:
                    accessLevel = User.userRights.Developer;
                    ErrorLog( new ArgumentOutOfRangeException( "command", typename , "not found in commandlist")  );
                    break;
            }
            return accessLevel;
        }

        public static void removeItemFromArray(string item, ref string[] array)
        {
            int count = 0;
            foreach( string i in array )
            {
                if( i == item )
                    count++;
            }

            string[ ] newArray = new string[ array.Length - count ];

            int nextAddition = 0;

            foreach( string  i in array )
            {
                if( i != item )
                {
                    newArray[ nextAddition ] = i;
                    nextAddition++;
                }
            }

            array = newArray;
        }

        public static int RealArrayLength( string[ ] args )
        {
            int argsLength = 0;
            foreach( string arg in args )
            {
                if( !string.IsNullOrEmpty( arg ) )
                    argsLength++;
            }
            return argsLength;
        }

        public static void silentPrivmsg( IAL irc, string channel, string message )
        {
            if( Configuration.Singleton( ).retrieveLocalStringOption( "silence", channel ) == "true" )
                return;
            else
                irc.IrcPrivmsg( channel, message );
        }
    }
}
