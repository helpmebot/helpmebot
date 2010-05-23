using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the edit count of a wikipedian
    /// </summary>
    class Editcount : GenericCommand
    {
        public Editcount( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            if( args.Length > 0 )
            {
                string userName = string.Join( " " , args );
                int editCount = getEditCount( userName, channel );
                if( editCount == -1 )
                {
                    string[ ] messageParams = { userName };
                    string message = Configuration.Singleton( ).GetMessage( "noSuchUser", messageParams );
                    return new CommandResponseHandler( message );
                }
                else
                {


                    string[ ] messageParameters = { editCount.ToString( ) , userName };

                    string message = Configuration.Singleton( ).GetMessage( "editCount" , messageParameters );

                    return new CommandResponseHandler( message );
                }
            }
            else
            {
                string[ ] messageParameters = { "count" , "1" , args.Length.ToString( ) };
                Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );
            }
            return null;
        }

        public int getEditCount( string username, string channel )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            if( username == string.Empty )
            {
                throw new ArgumentNullException( );
            }

            string baseWiki = Configuration.Singleton( ).retrieveLocalStringOption( "baseWiki", channel );

            DAL.Select q = new DAL.Select( "site_api" );
            q.setFrom( "site" );
            q.addWhere( new DAL.WhereConds( "site_id", baseWiki ) );
            string api = DAL.Singleton( ).executeScalarSelect( q );

            System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader( HttpRequest.get( api + "?format=xml&action=query&list=users&usprop=editcount&format=xml&ususers=" + username) );
            do
            {
                creader.Read( );
            } while( creader.Name != "user" );
            string editCount = creader.GetAttribute( "editcount" );
            if( editCount != null )
            {
                return int.Parse( editCount );
            }
            else
            {
                if( creader.GetAttribute( "missing" ) == "" )
                {
                    return -1;
                }
                else
                {
                    throw new ArgumentException( );
                }
            }


        }
    }
}
