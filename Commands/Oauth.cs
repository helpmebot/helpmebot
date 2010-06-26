#region Usings

using System;
using System.Net;
using System.Reflection;
using Twitterizer;
#endregion

namespace helpmebot6.Commands
{
    internal class Oauth : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            Twitter twit = Helpmebot6.twitter;

            twit.authorise( args[ 0 ] );

            return new CommandResponseHandler( Configuration.singleton( ).getMessage( "done" ) );
        }
    }
}