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
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            return Learn( word, phrase, false );
        }
        public static bool Learn( string word, string phrase, bool action )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            DAL.Select q = new DAL.Select( "COUNT(*)" );
            q.setFrom( "keywords" );
            q.addLimit( 1, 0 );
            q.addWhere( new DAL.WhereConds( "keyword_name", word ) );


            if( DAL.Singleton( ).executeScalarSelect( q ) != "0" )
                return false;

            DAL.Singleton( ).Insert( "keywords", "", word, phrase, ( action ? "1" : "0 " ) );
            return true;
        }

        public static RemeberedWord Remember( string word )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            DAL.Select q = new DAL.Select( "keyword_action" );
            q.setFrom( "keywords" );
            q.addWhere( new DAL.WhereConds( "keyword_name", word ) );

            string action = DAL.Singleton( ).executeScalarSelect( q );
            q = new DAL.Select( "keyword_response" );
            q.setFrom( "keywords" );
            q.addWhere( new DAL.WhereConds( "keyword_name", word ) );
            string result = DAL.Singleton( ).executeScalarSelect( q );

            RemeberedWord rW = new RemeberedWord( );
            rW.action = ( action == "1" ? true : false );
            rW.phrase = result;

            //  string result =  DAL.Singleton().ExecuteScalarQuery( "SELECT k.`keyword_response` FROM u_stwalkerster_hmb6.keywords k WHERE k.`keyword_name` = \"" + GlobalFunctions.escape( word ) + "\";" );
            return rW;
        }

        public static bool Forget( string word )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            DAL.Select q = new DAL.Select( "COUNT(*)" );
            q.setFrom( "keywords" );
            q.addWhere( new DAL.WhereConds( "keyword_name", word ) );

            if( DAL.Singleton( ).executeScalarSelect( q ) == "0" )
                return false;

            DAL.Singleton( ).Delete( "keywords", 0, new DAL.WhereConds( "keyword_name", word ) );
            return true;
        }

        public struct RemeberedWord
        {
            public string phrase;
            public bool action;
        }

    }
}