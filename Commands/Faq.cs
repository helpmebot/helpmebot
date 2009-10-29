using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Talks to the Nubio(squared) API to retrive FAQ information
    /// </summary>
    class Faq : GenericCommand
    {
        public Faq( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "faq" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            string command = GlobalFunctions.popFromFront( ref args );


            NubioApi faqRepo = new NubioApi( new Uri( Configuration.Singleton().retrieveGlobalStringOption( "faqApiUri" ) ) );
            string result;
            switch( command )
            {
                case "search":
                    result = faqRepo.searchFaq( string.Join( " " , args ) );
                    if( result != null )
                    {
                        Helpmebot6.irc.IrcPrivmsg( destination , result );
                    }
                    break;
                case "fetch":
                    result = faqRepo.fetchFaqText( int.Parse( args[ 0 ] ) );
                    if( result != null )
                    {
                        Helpmebot6.irc.IrcPrivmsg( destination , result );
                    }
                    break;
                case "link":
                    result = faqRepo.viewLink( int.Parse( args[ 0 ] ) );
                    if( result != null )
                    {
                        Helpmebot6.irc.IrcPrivmsg( destination , result );
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
