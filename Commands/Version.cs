using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;

namespace helpmebot6.Commands
{
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

        protected override void execute( User source , string destination , string[ ] args )
        {
            IAL.singleton.IrcPrivmsg( destination , getVersionString( ) );
        }

        public string getVersionString( )
        {
            SvnClient svn = new SvnClient( );

            SvnTarget tgt = SvnTarget.FromString( "../Helpmebot.cs" );

            SvnInfoEventArgs info;

            svn.GetInfo( tgt , out info );

            long rev = info.Revision;

            string branch = info.Uri.PathAndQuery.Substring( info.Uri.PathAndQuery.LastIndexOf( '/' ) );

            string versionString = this.version + "-" + branch + "-r" + rev.ToString();

            return versionString;
        }
    }
}
