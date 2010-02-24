using System;
namespace helpmebot6.Commands
{
    abstract class GenericCommand
    {
        /// <summary>
        /// Access level of the command
        /// </summary>

        public User.userRights accessLevel
        {
            get
            {
                string command = this.GetType( ).ToString( );
                string[ ] wc = { "typename = \"" + command + "\"" };
                string al = DAL.Singleton( ).Select( "accesslevel", "command", null, wc, null, null, null, 1, 0 );
                try
                {
                   return (User.userRights)Enum.Parse( typeof( User.userRights ), al, true );
                }
                catch( ArgumentException )
                {
                    Logger.Instance( ).addToLog( "Warning: " + command + " not found in access list.", Logger.LogTypes.ERROR );
                   return   User.userRights.Developer;

                }
            }
        }

        /// <summary>
        /// Trigger an exectution of the command
        /// </summary>
        /// <param name="source"></param>
        /// <param name="channel"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public CommandResponseHandler run( User source, string channel, string[ ] args )
        {
            string command = this.GetType( ).ToString( );

            Log( "Running command: " + command );

            return accessTest( source, channel, args );
        }

        /// <summary>
        /// Check the access level and then decide what to do.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="channel"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual CommandResponseHandler accessTest( User source, string channel, string[ ] args )
        {
            // check the access level
            if( source.AccessLevel < accessLevel )
            {
                return accessDenied( source, channel, args );
            }
            else
            {
                return reallyRun( source, channel, args );
            }
        }

        /// <summary>
        /// Access granted to command, decide what to do
        /// </summary>
        /// <param name="source"></param>
        /// <param name="channel"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual CommandResponseHandler reallyRun( User source, string channel, string[ ] args )
        {
            AccessLog.instance( ).Save( new AccessLog.AccessLogEntry( source, this.GetType( ), true ) );
            Log( "Starting command execution..." );
            CommandResponseHandler crh;
            try
            {
                crh = execute( source, channel, args );
            }
            catch( Exception ex )
            {
                Logger.Instance( ).addToLog( ex.ToString( ), Logger.LogTypes.ERROR );
                crh = new CommandResponseHandler( ex.Message );
            }
            Log( "Command execution complete." );
            return crh;
        }

        /// <summary>
        /// Access denied to command, decide what to do
        /// </summary>
        /// <param name="source"></param>
        /// <param name="channel"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual CommandResponseHandler accessDenied( User source, string channel, string[ ] args )
        {
            CommandResponseHandler response = new CommandResponseHandler( );

            response.respond( Configuration.Singleton( ).GetMessage( "accessDenied", "" ), CommandResponseDestination.PRIVATE_MESSAGE );
            Log( "Access denied to command." );
            AccessLog.instance( ).Save( new AccessLog.AccessLogEntry( source, this.GetType( ), false ) );
            return response;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source"></param>
        /// <param name="channel"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected abstract CommandResponseHandler execute( User source, string channel, string[ ] args );

        protected void Log( string message )
        {
            Logger.Instance( ).addToLog( message, Logger.LogTypes.COMMAND );
        }

    }
}
