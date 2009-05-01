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
            DAL.singleton.ExecuteNonQuery( "INSERT INTO `u_stwalkerster_hmb6`.`keywords` (`keyword_name`,`keyword_response`) VALUES (\"" + GlobalFunctions.escape( word ) + "\",\"" + GlobalFunctions.escape( phrase ) + "\");" );
        }

        public static string Remember( string word )
        {
            string result =  DAL.singleton.ExecuteScalarQuery( "SELECT k.`keyword_response` FROM u_stwalkerster_hmb6.keywords k WHERE k.`keyword_name` = \"" + GlobalFunctions.escape( word ) + "\";" );
            return result;
        }

    }
}
