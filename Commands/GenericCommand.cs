using System;
namespace helpmebot6.Commands
{
    abstract class GenericCommand
    {

        public User.userRights accessLevel = User.userRights.Normal;

        public CommandResponseHandler run( User source, string channel, string[ ] args )
        {
            string command = this.GetType( ).ToString( );

            Log( "Running command: " + command );

            // get the access level of this command

            string[ ] wc = { "typename = \"" + command + "\"" };
            string al = DAL.Singleton( ).Select( "accesslevel", "command", null, wc, null, null, null, 1, 0 );
            try
            {
                accessLevel = (User.userRights)Enum.Parse( typeof( User.userRights ), al, true );
            }
            catch( ArgumentException )
            {
                accessLevel = User.userRights.Developer;
                Logger.Instance( ).addToLog( "Warning: " + command + " not found in access list.", Logger.LogTypes.ERROR );
            }

            // check the access level
            if( source.AccessLevel < accessLevel )
            {
                CommandResponseHandler response = new CommandResponseHandler( );


                response.respond( Configuration.Singleton( ).GetMessage( "accessDenied", "" ), CommandResponseDestination.PRIVATE_MESSAGE );
                Log( "Access denied to command." );
                AccessLog.instance( ).Save( new AccessLog.AccessLogEntry( source, this.GetType( ), false ) );
                return response;
            }
            else
            {
                AccessLog.instance( ).Save( new AccessLog.AccessLogEntry( source, this.GetType( ), true ) );
                Log( "Starting command execution..." );
                CommandResponseHandler crh;
                try 
                {
                    crh = execute(source, channel, args);
                }
                catch (Exception ex)
                {
                    Logger.Instance().addToLog(ex.ToString(),Logger.LogTypes.ERROR);
                    crh = new CommandResponseHandler(ex.Message);
                }
                Log( "Command execution complete." );
                return crh;
            }
        }

        protected abstract CommandResponseHandler execute( User source, string channel, string[ ] args );

        protected void Log( string message )
        {
            Logger.Instance( ).addToLog( message, Logger.LogTypes.COMMAND );
        }

    }
}
