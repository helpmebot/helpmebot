using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
namespace helpmebot6.Commands
{
   abstract class GenericCommand
    {

      public User.userRights accessLevel = User.userRights.Normal;

       public void run( User source, string destination, object[ ] args )
       {
           if( User.userRights < accessLevel )
           {
               IAL.singleton.IrcPrivmsg( source.Nickname , Configuration.Singleton( ).GetMessage( "accessDenied" , "" ) );
               string[ ] aDArgs = { source.ToString( ) , MethodBase.GetCurrentMethod( ).Name };
               IAL.singleton.IrcPrivmsg( Configuration.Singleton( ).retrieveStringOption( "channelDebug" ) , Configuration.Singleton( ).GetMessage( "accessDeniedDebug" , aDArgs ) );
           }
           else
           {
               execute( source , destination , args );
           }
       }

       abstract void execute( User source , string destination , string[ ] args );
    }
}
