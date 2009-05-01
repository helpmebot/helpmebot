using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace helpmebot6
{
    public class CommandParser
    {
        public static CommandParser singleton;

        Configuration config = Configuration.singleton;

        public delegate void CommandEventHandler( User source, string destination , string command, string[ ] args );
        public event CommandEventHandler CommandRecievedEvent;
        public event CommandEventHandler FaqCommandRecievedEvent;

        public CommandParser( )
        {
            CommandRecievedEvent += new CommandEventHandler( CommandParser_CommandRecievedEvent );
            FaqCommandRecievedEvent += new CommandEventHandler( CommandParser_FaqCommandRecievedEvent );
        }

        void CommandParser_FaqCommandRecievedEvent( User source, string destination, string command, string[ ] args )
        {
            NubioApi faqRepo = new NubioApi( new Uri( config.retrieveStringOption( "faqApiUri" ) ) );
            string result;
            switch ( command )
            {
                case "search":
                    result = faqRepo.searchFaq( string.Join( " ", args ) );
                    if ( result != null )
                    {
                        IAL.singleton.IrcPrivmsg( destination, result );
                    }
                    break;
                case "fetch":
                    result = faqRepo.fetchFaqText( int.Parse( args[ 0 ] ) );
                    if ( result != null )
                    {
                        IAL.singleton.IrcPrivmsg( destination, result );
                    }
                    break;
                case "link":
                    result = faqRepo.viewLink( int.Parse( args[ 0 ] ) );
                    if ( result != null )
                    {
                        IAL.singleton.IrcPrivmsg( destination, result );
                    }
                    break;
                default:
                    break;
            }
        }

        public void CommandParser_CommandRecievedEvent( User source, string destination, string command, string[ ] args )
        {
            if ( source.AccessLevel == User.userRights.Ignored )
            {
                Console.WriteLine( "IGNORED MESSAGE FROM " + source.ToString( ) + " COMMAND " + command + " TO " + destination + " ARGS " + string.Join( " ", args ) );
                return;
            }

            // Check for a learned word
            string wordResponse =WordLearner.Remember(command);
            if ( wordResponse != null )
            {
                IAL.singleton.IrcPrivmsg( destination, wordResponse );
            }

            // Not a word, check for a valid command

            switch ( command )
            {
                case "sayhi":
                    this.SayHi( source.Nickname, destination );
                    break;
                case "faq":
                    command = GlobalFunctions.popFromFront(ref args);
                    FaqCommandRecievedEvent( source, destination, command, args );
                    break;
                default:
                    break;
            }
        }



        public void SayHi( string toUser, string destination )
        {
            IAL.singleton.IrcPrivmsg( 
                destination, 
                Configuration.singleton.GetMessage("cmdSayHi1", 
                    toUser
                    )
                );
        }

    }
}
