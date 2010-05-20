using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

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

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            if( GlobalFunctions.isInArray( "@svn", args ) != -1 )
                return new CommandResponseHandler( getVersionString( ) );
            else
                return new CommandResponseHandler( version );
        }

        public string getVersionString( )
        {

            string rev = Process.Start( "svnversion" ).StandardOutput.ReadLine();
            
            string versionString = this.version +  "-r" + rev;

            return versionString;
        }
    }
}
