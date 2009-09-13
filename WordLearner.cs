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
        public static void Learn( string word, string phrase )
        {
            DAL.Singleton().ExecuteNonQuery( "INSERT INTO `keywords` (`keyword_name`,`keyword_response`) VALUES (\"" + GlobalFunctions.escape( word ) + "\",\"" + GlobalFunctions.escape( phrase ) + "\");" );
        }

        public static string Remember( string word )
        {
            string[] whereconds = {"keyword_name = \""+word+"\""};
            string result = DAL.Singleton( ).Select("keyword_response","keywords",new DAL.join[0],whereconds,new string[0],new DAL.order[0],new string[0],0,0);

          //  string result =  DAL.Singleton().ExecuteScalarQuery( "SELECT k.`keyword_response` FROM u_stwalkerster_hmb6.keywords k WHERE k.`keyword_name` = \"" + GlobalFunctions.escape( word ) + "\";" );
            return result;
        }

        public static void Forget( string word )
        {
            DAL.Singleton( ).ExecuteNonQuery( "DELETE FROM `keywords` WHERE `keyword_name` = '" + word + "';" );
        }

    }
}
