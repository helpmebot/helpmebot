#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Learns a keyword
    /// </summary>
    internal class Learn : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            bool action = false;
            if (args[0] == "@action")
            {
                action = true;
                GlobalFunctions.popFromFront(ref args);
            }

            if (args.Length >= 2)
            {
                Helpmebot6.irc.ircNotice( source.nickname,
                                          WordLearner.learn( args[ 0 ],
                                                             string.Join( " ",
                                                                          args,
                                                                          1,
                                                                          args.
                                                                              Length -
                                                                          1 ),
                                                             action )
                                              ? Configuration.singleton( ).
                                                    getMessage( "cmdLearnDone" )
                                              : Configuration.singleton( ).
                                                    getMessage( "cmdLearnError" ) );
            }
            else
            {
                string[] messageParameters = {"learn", "2", args.Length.ToString()};
                Helpmebot6.irc.ircNotice(source.nickname,
                                         Configuration.singleton().getMessage("notEnoughParameters", messageParameters));
            }
            return null;
        }
    }
}