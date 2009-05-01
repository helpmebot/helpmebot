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
