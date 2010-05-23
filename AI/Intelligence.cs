using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.AI
{
    class Intelligence
    {
        private static Intelligence singleton;
        public static Intelligence Singleton( )
        {
            if( singleton == null )
                singleton = new Intelligence( );

            return singleton;
        }

        protected Intelligence( )
        {

        }

        public string Respond( string input )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            string[ ] tokens = input.Split( ' ' );

            string[ ] hello = { "hi" , "hey" , "heya" , "hello" };
            string helloResponses = "cmdSayHi1";
            string[ ] morning = { "morning" , "good morning" };
            string morningResponses = "cmdSayHiMorning";
            string[ ] afternoon = { "afternoon" , "good afternoon" };
            string afternoonResponses = "cmdSayHiAfternoon";
            string[ ] evening = { "evening" , "good evening" };
            string eveningResponses = "cmdSayHiEvening";

            foreach( string word in hello )
            {
                if( tokens.Length > 1 )
                {
                    if( tokens[ 0 ] == word && tokens[ 1 ] == Helpmebot6.irc.IrcNickname )
                    {
                        return helloResponses;
                    }
                }
            }

            foreach( string word in morning )
            {
                if( tokens.Length > 1 )
                {
                    if( tokens[ 0 ] == word && tokens[ 1 ] == Helpmebot6.irc.IrcNickname )
                    {
                        return morningResponses;
                    }
                }
            }

            foreach( string word in afternoon )
            {
                if( tokens.Length > 1 )
                {
                    if( tokens[ 0 ] == word && tokens[ 1 ] == Helpmebot6.irc.IrcNickname )
                    {
                        return afternoonResponses;
                    }
                }
            }

            foreach( string word in evening )
            {
                if( tokens.Length > 1 )
                {
                    if( tokens[ 0 ] == word && tokens[ 1 ] == Helpmebot6.irc.IrcNickname )
                    {
                        return eveningResponses;
                    }
                }
            }

            return "";
        }

    }
}
