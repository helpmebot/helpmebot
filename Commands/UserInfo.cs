using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{

    // returns information about a user
    // what                 how                     info    message

    // contribs link        [calc]                  Done    Done
    // last contrib
    // userpage             [calc]                  Done    Done
    // usertalkpage         [calc]                  Done    Done
    // editcount            Commands.Count          Done    Done
    // registration date    Commands.Registration   Done    Done
    // block log
    // block status
    // user groups          Commands.Rights         Done    Done
    // editrate (edits/days) Commands.Age           Done 
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

                string userPageUrl = getUserPageUrl( userName );
                string userTalkPageUrl = getUserTalkPageUrl( userName );
                string userContributionsUrl = getUserContributionsUrl( userName );

                Age ageCommand = new Age( );
                TimeSpan wikipedianAge = ageCommand.getWikipedianAge( userName );
                ageCommand = null;

                double editRate = editCount / wikipedianAge.TotalDays;

                IAL irc = IAL.singleton;

//##################################################
                

                irc.IrcNotice( source.Nickname , userPageUrl );
                irc.IrcNotice( source.Nickname , userTalkPageUrl );
                irc.IrcNotice( source.Nickname , userContributionsUrl );

                string message = "";
                if( userRights != "" )
                {
                  string[ ]  messageParameters = { userName , userRights };
                    message = Configuration.Singleton( ).GetMessage( "cmdRightsList" , messageParameters );

                }
                else
                {
                    message = Configuration.Singleton( ).GetMessage( "cmdRightsNone" , userName );
                }
                irc.IrcNotice( source.Nickname , message );

               string[ ]  messageParameters2 = { editCount.ToString() , userName };
                message = Configuration.Singleton( ).GetMessage( "editCount" , messageParameters2 );
                irc.IrcNotice( source.Nickname , message );

               string[] messageParameters3 = { userName , registrationDate.ToString( "hh:mm:ss t" ) , registrationDate.ToString( "d MMMM yyyy" ) };
                 message = Configuration.Singleton( ).GetMessage( "registrationDate" , messageParameters3 );
                 irc.IrcNotice( source.Nickname , message );
                 string[ ] messageParameters4 = { userName , editRate.ToString() };
                 message = Configuration.Singleton( ).GetMessage( "editRate" , messageParameters4 );
                 irc.IrcNotice( source.Nickname , message );
            }
            else
            {
                string[ ] messageParameters = { "userinfo" , "1" , args.Length.ToString( ) };
                IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );

            }
        }

        private string getUserPageUrl( string userName )
        {
            // look up site id
            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );
            
            // get api
            string api = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_api` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // api-> get mainpage name (Mediawiki:mainpage)
            string apiQuery = "?action=query&prop=revisions&titles=Mediawiki:Mainpage&rvprop=content&format=xml";
            System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader( api + apiQuery );
            do
            {
                creader.Read( );
            } while( creader.Name != "rev" );
            string mainpagename = creader.ReadContentAsString( );

            mainpagename = mainpagename.Replace( " " , "_" );
           
            // get mainpage url from site table
            string mainpageurl = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_mainpage` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace( " " , "_" );

            return mainpageurl.Replace( mainpagename , "User:" + userName );
        }
        private string getUserTalkPageUrl( string userName )
        {
            // look up site id
            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );

            // get api
            string api = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_api` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // api-> get mainpage name (Mediawiki:mainpage)
            string apiQuery = "?action=query&prop=revisions&titles=Mediawiki:Mainpage&rvprop=content&format=xml";
            System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader( api + apiQuery );
            do
            {
                creader.Read( );
            } while( creader.Name != "rev" );
            string mainpagename = creader.ReadContentAsString( );

            mainpagename = mainpagename.Replace( " " , "_" );

            // get mainpage url from site table
            string mainpageurl = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_mainpage` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace( " " , "_" );

            return mainpageurl.Replace( mainpagename , "User_talk:" + userName );
        }
        private string getUserContributionsUrl( string userName )
        {
            // look up site id
            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );

            // get api
            string api = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_api` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // api-> get mainpage name (Mediawiki:mainpage)
            string apiQuery = "?action=query&prop=revisions&titles=Mediawiki:Mainpage&rvprop=content&format=xml";
            System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader( api + apiQuery );
            do
            {
                creader.Read( );
            } while( creader.Name != "rev" );
            string mainpagename = creader.ReadContentAsString( );

            mainpagename = mainpagename.Replace( " " , "_" );

            // get mainpage url from site table
            string mainpageurl = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_mainpage` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace( " " , "_" );

            return mainpageurl.Replace( mainpagename , "Special:Contributions/" + userName );
        }

    }
}
