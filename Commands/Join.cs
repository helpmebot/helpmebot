#region Usings

using System.Collections.Generic;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Joins an IRC channel
    /// </summary>
    internal class Join : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            DAL.Select q = new DAL.Select("count(*)");
            q.addWhere(new DAL.WhereConds("channel_name", args[0]));
            q.addWhere(new DAL.WhereConds("channel_network", source.network.ToString()));
            q.setFrom("channel");

            string count = DAL.singleton().executeScalarSelect(q);


            if (count == "1")
            {
                // entry exists

                Dictionary<string, string> vals = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "channel_enabled",
                                                              "1"
                                                              }
                                                      };
                DAL.singleton().update("channel", vals, 1, new DAL.WhereConds("channel_name", args[0]));

                Helpmebot6.irc.ircJoin(args[0]);
            }
            else
            {
                DAL.singleton().insert("channel", "", args[0], "", "1", source.network.ToString());
                Helpmebot6.irc.ircJoin(args[0]);
            }
            return null;
        }
    }
}