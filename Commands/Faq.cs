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

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            string command = GlobalFunctions.popFromFront( ref args );
            CommandResponseHandler crh = new CommandResponseHandler( );

            NubioApi faqRepo = new NubioApi( new Uri( Configuration.Singleton().retrieveGlobalStringOption( "faqApiUri" ) ) );
            string result;
            switch( command )
            {
                case "search":
                    result = faqRepo.searchFaq( string.Join( " " , args ) );
                    if( result != null )
                    {
                        crh.respond( result );
                    }
                    break;
                case "fetch":
                    result = faqRepo.fetchFaqText( int.Parse( args[ 0 ] ) );
                    if( result != null )
                    {
                        crh.respond( result );
                    }
                    break;
                case "link":
                    result = faqRepo.viewLink( int.Parse( args[ 0 ] ) );
                    if( result != null )
                    {
                        crh.respond( result );
                    }
                    break;
                default:
                    break;
            }

            return crh;
        }
    }
}
