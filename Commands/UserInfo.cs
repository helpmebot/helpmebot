using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{

    // returns information about a user

    // contribs             [calc]
    // last contrib
    // userpage             [calc]
    // usertalkpage         [calc]
    // editcount            Commands.Count
    // registration date    Commands.Registration
    // block log
    // block status
    // user groups          Commands.Rights
    // editrate (edits/days) [calc]
    class UserInfo : GenericCommand
    {
        public UserInfo( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "userinfo" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            if( args.Length > 0 )
            {
                string userName = string.Join( " " , args );

                Rights rightsCommand = new Rights( );
                string userRights = rightsCommand.getRights( userName );
                rightsCommand = null;

                Count countCommand = new Count( );
                int editCount = countCommand.getEditCount( userName );
                countCommand = null;

                Registration registrationCommand = new Registration( );
                DateTime registrationDate = registrationCommand.getRegistrationDate( userName );
                registrationCommand = null;



            }
            else
            {
                string[ ] messageParameters = { "userinfo" , "1" , args.Length.ToString( ) };
                IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );

            }
        }

        private string getUserPageUrl( string userName , int site )
        {
            // look up site id
            // get api
            // api-> get mainpage name (Mediawiki:mainpage)
            // get mainpage url from site table
            // replace mainpage in mainpage url with user:<username>
            // 
            throw new NotImplementedException( );
        }

    }
}
