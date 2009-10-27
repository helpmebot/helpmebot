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

namespace helpmebot6
{
    public class WordLearner
    {
        public static bool Learn( string word, string phrase )
        {
            return Learn( word , phrase , false );
        }
        public static bool Learn(string word, string phrase, bool action)
        {
            string[] wc = {"keyword_name = \""+word+"\""};
            if( DAL.Singleton( ).Select( "COUNT(*)" , "keywords" , null , wc , null , null , null , 1 , 0 ) != "0" )
                return false;

            DAL.Singleton( ).ExecuteNonQuery( "INSERT INTO `keywords` (`keyword_name`,`keyword_response`, `keyword_action`) VALUES (\"" + GlobalFunctions.escape( word ) + "\",\"" + GlobalFunctions.escape( phrase ) + "\", " + ( action ? 1 : 0 ) + ");" );
            return true;
        }

        public static RemeberedWord Remember( string word )
        {
            string[] whereconds = {"keyword_name = \""+word+"\""};
            string action = DAL.Singleton( ).Select( "keyword_action" , "keywords" , new DAL.join[ 0 ] , whereconds , new string[ 0 ] , new DAL.order[ 0 ] , new string[ 0 ] , 0 , 0 );
            string result = DAL.Singleton( ).Select("keyword_response","keywords",new DAL.join[0],whereconds,new string[0],new DAL.order[0],new string[0],0,0);

            RemeberedWord rW = new RemeberedWord( );
            rW.action = ( action == "1" ? true : false );
            rW.phrase = result;

          //  string result =  DAL.Singleton().ExecuteScalarQuery( "SELECT k.`keyword_response` FROM u_stwalkerster_hmb6.keywords k WHERE k.`keyword_name` = \"" + GlobalFunctions.escape( word ) + "\";" );
            return rW;
        }

        public static bool Forget( string word )
        {
            string[ ] wc = { "keyword_name = \"" + word + "\"" };
            if( DAL.Singleton( ).Select( "COUNT(*)" , "keywords" , null , wc , null , null , null , 1 , 0 ) == "0" )
                return false;

            DAL.Singleton( ).ExecuteNonQuery( "DELETE FROM `keywords` WHERE `keyword_name` = '" + word + "';" );
            return true;
        }

        public struct RemeberedWord
        {
            public string phrase;
            public bool action;
        }

    }
}
