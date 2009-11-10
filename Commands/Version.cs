using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the current version of the bot.
    /// </summary>
    class Version : GenericCommand
    {

        public string version
        {
            get
            {
                return "6.0";
            }
        }

        public Version( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "version" );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            return new CommandResponseHandler( getVersionString( ) );
        }

        public string getVersionString( )
        {
            //SvnClient svn = new SvnClient( );

            //SvnTarget tgt = SvnTarget.FromString( "../Helpmebot.cs" );

            //SvnInfoEventArgs info;

            //svn.GetInfo( tgt , out info );

            long rev = 0; //info.Revision;

            string branch = "trunk"; //info.Uri.PathAndQuery.Substring( info.Uri.PathAndQuery.LastIndexOf( '/' ) );

            string versionString = this.version + "-" + branch + "-r" + rev.ToString();

            return versionString;
        }
    }
}
