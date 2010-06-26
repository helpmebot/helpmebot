#region Usings

using System;
using System.Net;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Tweet : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            Twitter twit = new Twitter(
                Configuration.singleton().retrieveGlobalStringOption("twitterUsername"),
                Configuration.singleton().retrieveGlobalStringOption("twitterPassword")
                )
                               {
                                   userAgent =
                                       Configuration.singleton( ).
                                       retrieveGlobalStringOption( "useragent" )
                               };

            string status = string.Join(" ", args);

            HttpStatusCode wrsp = twit.statuses_update(status);
            if (wrsp == HttpStatusCode.OK)
                return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
            throw new Exception();
        }
    }
}