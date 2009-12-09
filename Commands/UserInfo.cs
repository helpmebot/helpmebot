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
    // block log            [calc]                  Done    Done
    // block status
    // user groups          Commands.Rights         Done    Done
    // editrate (edits/days) Commands.Age           Done    Done

            


    /// <summary>
    /// Returns the user information about a specified user
    /// </summary>
    class Userinfo : GenericCommand
    {
        CommandResponseHandler crh = new CommandResponseHandler( );

        public Userinfo( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            bool useLongInfo = bool.Parse( Configuration.Singleton( ).retrieveLocalStringOption( "useLongUserInfo" , channel ) );

            if( args.Length > 0 )
            {
                if( args[ 0 ] == "@long" )
                {
                    useLongInfo = true;
                    GlobalFunctions.popFromFront( ref args );
                }
                if( args[ 0 ] == "@short" )
                {
                    useLongInfo = false;
                    GlobalFunctions.popFromFront( ref args );
                }
            }

            if( args.Length > 0 )
            {
                string userName = string.Join( " " , args );

                UserInformation uInfo = new UserInformation( );

                Count countCommand = new Count( );
                uInfo.editCount= countCommand.getEditCount( userName );
                countCommand = null;

                if( uInfo.editCount == -1 )
                {
                    string[] mparams = { userName };
                    crh.respond( Configuration.Singleton( ).GetMessage( "noSuchUser" , mparams ) );
                    return crh;
                }

                retrieveUserInformation( userName , ref uInfo );


                //##################################################


                if( useLongInfo )
                {
                    sendLongUserInfo( uInfo );
                }
                else
                {
                    sendShortUserInfo( uInfo );
                }
            }
            else
            {
                string[ ] messageParameters = { "userinfo" , "1" , args.Length.ToString( ) };
                Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );

            }
            return crh;
            
        }

        private string getUserPageUrl( string userName )
        {
            if( userName == string.Empty )
            {
                throw new ArgumentNullException( );
            }
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
            string mainpagename = creader.ReadElementContentAsString( );

            mainpagename = mainpagename.Replace( " " , "_" );
           
            // get mainpage url from site table
            string mainpageurl = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_mainpage` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace( " " , "_" );

            return mainpageurl.Replace( mainpagename , "User:" + userName );
        }
        private string getUserTalkPageUrl( string userName )
        {
            if( userName == string.Empty )
            {
                throw new ArgumentNullException( );
            }
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
            string mainpagename = creader.ReadElementContentAsString( );

            mainpagename = mainpagename.Replace( " " , "_" );

            // get mainpage url from site table
            string mainpageurl = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_mainpage` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace( " " , "_" );

            return mainpageurl.Replace( mainpagename , "User_talk:" + userName );
        }
        private string getUserContributionsUrl( string userName )
        {
            if( userName == string.Empty )
            {
                throw new ArgumentNullException( );
            }
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
            string mainpagename = creader.ReadElementContentAsString( );

            mainpagename = mainpagename.Replace( " " , "_" );

            // get mainpage url from site table
            string mainpageurl = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_mainpage` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace( " " , "_" );

            return mainpageurl.Replace( mainpagename , "Special:Contributions/" + userName );
        }
        private string getBlockLogUrl( string userName )
        {
            if( userName == string.Empty )
            {
                throw new ArgumentNullException( );
            }
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
            string mainpagename = creader.ReadElementContentAsString( );

            mainpagename = mainpagename.Replace( " " , "_" );

            // get mainpage url from site table
            string mainpageurl = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_mainpage` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace( " " , "_" );

            return mainpageurl.Replace( mainpagename , "Special:Log?type=block&page=User:" + userName );
        }

        private UserInformation retrieveUserInformation( string userName, ref UserInformation initial )
        {
            try
            {
                initial.userName = userName;

                if( initial.editCount == 0 )
                {
                    Count countCommand = new Count( );
                    initial.editCount = countCommand.getEditCount( userName );
                    countCommand = null;
                }

                Rights rightsCommand = new Rights( );
                initial.userGroups = rightsCommand.getRights( userName );
                rightsCommand = null;

                Registration registrationCommand = new Registration( );
                initial.registrationDate = registrationCommand.getRegistrationDate( userName );
                registrationCommand = null;

                initial.userPage = getUserPageUrl( userName );
                initial.talkPage = getUserTalkPageUrl( userName );
                initial.userContribs = getUserContributionsUrl( userName );
                initial.userBlockLog = getBlockLogUrl( userName );

                Age ageCommand = new Age( );
                initial.userAge= ageCommand.getWikipedianAge( userName );
                ageCommand = null;

              initial.editRate = initial.editCount / initial.userAge.TotalDays;


                return initial;
            }
            catch( NullReferenceException ex )
            {
                GlobalFunctions.ErrorLog( ex  );
                throw new InvalidOperationException( );
            }
            
        }

        private void sendShortUserInfo( UserInformation userInformation)
        {
            string regex = "^http://en.wikipedia.org/wiki/";
            string shortUrlAlias = "http://enwp.org/";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex( regex );

            userInformation.userPage = r.Replace( userInformation.userPage , shortUrlAlias );
            userInformation.talkPage = r.Replace( userInformation.talkPage , shortUrlAlias );
            userInformation.userBlockLog = r.Replace( userInformation.userBlockLog , shortUrlAlias );
            userInformation.userContribs = r.Replace( userInformation.userContribs , shortUrlAlias );

            string[ ] messageParameters = {
                                userInformation.userName,
                                userInformation.userPage, 
                                userInformation.talkPage, 
                                userInformation.userContribs, 
                                userInformation.userBlockLog,
                                userInformation.userGroups,
                                userInformation.userAge.ToString(),
                                userInformation.registrationDate.ToString(),
                                userInformation.editRate.ToString(),
                                userInformation.editCount.ToString()
                                         };

            string message = Configuration.Singleton( ).GetMessage( "cmdUserInfoShort" , messageParameters );

            crh.respond( message );
        }

        private void sendLongUserInfo( UserInformation userInformation )
        {
         

            crh.respond( userInformation.userPage );
            crh.respond( userInformation.talkPage );
            crh.respond( userInformation.userContribs );
            crh.respond( userInformation.userBlockLog );

            string message = "";
            if( userInformation.userGroups!= "" )
            {
                string[ ] messageParameters = { userInformation.userName , userInformation.userGroups};
                message = Configuration.Singleton( ).GetMessage( "cmdRightsList" , messageParameters );

            }
            else
            {
                message = Configuration.Singleton( ).GetMessage( "cmdRightsNone" , userInformation.userName );
            }
            crh.respond( message );

            string[ ] messageParameters2 = { userInformation.editCount.ToString( ) , userInformation.userName };
            message = Configuration.Singleton( ).GetMessage( "editCount" , messageParameters2 );
            crh.respond( message );

            string[ ] messageParameters3 = { userInformation.userName , userInformation.registrationDate.ToString( "hh:mm:ss t" ) , userInformation.registrationDate.ToString( "d MMMM yyyy" ) };
            message = Configuration.Singleton( ).GetMessage( "registrationDate" , messageParameters3 );
            crh.respond( message );
            string[ ] messageParameters4 = { userInformation.userName , userInformation.editRate.ToString( ) };
            message = Configuration.Singleton( ).GetMessage( "editRate" , messageParameters4 );
            crh.respond( message );
        }

        private struct UserInformation
        {
            public string userName;
            public int editCount;
            public string userGroups;
            public DateTime registrationDate;
            public double editRate;
            public string userPage;
            public string talkPage;
            public string userContribs;
            public string userBlockLog;
            public TimeSpan userAge;
        }
    }
}
