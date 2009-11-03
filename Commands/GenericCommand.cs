namespace helpmebot6.Commands
{
   abstract class GenericCommand
    {

      public User.userRights accessLevel = User.userRights.Normal;

      public CommandResponseHandler run( User source , string[ ] args )
       {
           string command = this.GetType( ).ToString( );

            Log( "Running command: " +command );

           if( source.AccessLevel < accessLevel )
           {
               CommandResponseHandler response = new CommandResponseHandler( );


               response.respond( Configuration.Singleton( ).GetMessage( "accessDenied" , "" ),CommandResponseDestination.PRIVATE_MESSAGE );
               string[ ] aDArgs = { source.ToString( ) ,  command };
               response.respond(  Configuration.Singleton( ).GetMessage( "accessDeniedDebug" , aDArgs ), CommandResponseDestination.CHANNEL_DEBUG );
               Log( "Access denied to command." );
               return response;
           }
           else
           {
               Log( "Starting command execution..." );
               CommandResponseHandler crh =  execute( source , args );
               Log( "Command execution complete." );
               return crh;
           }
       }

      protected abstract CommandResponseHandler execute( User source , string[ ] args );

      protected void Log( string message )
      {
          Logger.Instance( ).addToLog( message , Logger.LogTypes.COMMAND );
      }
   
    }
}
