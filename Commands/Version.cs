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

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            return new CommandResponseHandler( getVersionString( ) );
        }

        public string getVersionString( )
        {
            SvnClient svn = new SvnClient( );

            SvnTarget tgt = SvnTarget.FromString( "../../Helpmebot.cs" );

            SvnInfoEventArgs info;

            svn.GetInfo( tgt, out info );

            long rev = 0;
            rev = info.Revision;

            string branch = "trunk";
            string pq = info.Uri.PathAndQuery;
            char[ ] splitChars = { '/' };
            branch = pq.Split( splitChars, StringSplitOptions.RemoveEmptyEntries )[ pq.Split( splitChars, StringSplitOptions.RemoveEmptyEntries ).Length - 2 ];

            string versionString = this.version + "-" + branch + "-r" + rev.ToString( );

            return versionString;
        }
    }
}
