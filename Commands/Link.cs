#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    using System.Collections;
    using System.Linq;

    internal class Link : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                string.Format("Method:{0}{1}", MethodBase.GetCurrentMethod().DeclaringType.Name,
                              MethodBase.GetCurrentMethod().Name), Logger.LogTypes.DNWB);

            bool secure = bool.Parse(Configuration.singleton().retrieveLocalStringOption("useSecureWikiServer", channel));
            if (args.Length > 0)
            {
                if (args[0] == "@secure")
                {
                    secure = true;
                    GlobalFunctions.popFromFront(ref args);
                }
            }

            if (GlobalFunctions.realArrayLength(args) > 0)
            {
               ArrayList links = Linker.instance().reallyParseMessage(string.Join(" ", args));

                string message = links.Cast<string>( ).Aggregate( "", ( current, link ) => current + " "+ Linker.getRealLink( channel, link, secure ) );

                return new CommandResponseHandler(message);
            }
            return new CommandResponseHandler( Linker.instance( ).getLink( channel, secure ) );
        }
    }
}