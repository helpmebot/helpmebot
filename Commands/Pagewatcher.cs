#region Usings

using System.Reflection;
using helpmebot6.Monitoring.PageWatcher;

#endregion

namespace helpmebot6.Commands
{
    internal class Pagewatcher : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (args.Length >= 1)
            {
                switch (GlobalFunctions.popFromFront(ref args).ToLower())
                {
                    case "add":
                        return addPageWatcher(string.Join(" ", args), channel);
                    case "del":
                        return removePageWatcher(string.Join(" ", args), channel);
                    case "list":
                        CommandResponseHandler crh = new CommandResponseHandler();
                        foreach (string item in PageWatcherController.instance().getWatchedPages())
                        {
                            crh.respond(item);
                        }
                        return crh;
                }
            }
            return new CommandResponseHandler();
        }

        private static CommandResponseHandler addPageWatcher(string page, string channel)
        {
            DAL.Select q = new DAL.Select("COUNT(*)");
            q.setFrom("watchedpages");
            q.addWhere(new DAL.WhereConds("pw_title", page));

            // look to see if watchedpage exists
            if (DAL.singleton().executeScalarSelect(q) == "0")
            {
//    no: addOrder it
                DAL.singleton().insert("watchedpages", "", page);
            }

            // get id of watchedpage
            q = new DAL.Select("pw_id");
            q.setFrom("watchedpages");
            q.addWhere(new DAL.WhereConds("pw_title", page));

            string watchedPageId = DAL.singleton().executeScalarSelect(q);

            // get id of channel
            string channelId = Configuration.singleton().getChannelId(channel);

            // addOrder to pagewatcherchannels
            DAL.singleton().insert("pagewatcherchannels", channelId, watchedPageId);

            PageWatcherController.instance().loadAllWatchedPages();

            return null;
        }

        private static CommandResponseHandler removePageWatcher(string page, string channel)
        {
            // get id of watchedpage
            DAL.Select q = new DAL.Select("pw_id");
            q.setFrom("watchedpages");
            q.addWhere(new DAL.WhereConds("pw_title", page));

            string watchedPageId = DAL.singleton().executeScalarSelect(q);

            // get id of channel
            string channelId = Configuration.singleton().getChannelId(channel);

            // remove from pagewatcherchannels
            DAL.singleton().delete("pagewatcherchannels", 0, new DAL.WhereConds("pwc_channel", channelId),
                                   new DAL.WhereConds("pwc_pagewatcher", watchedPageId));

            PageWatcherController.instance().loadAllWatchedPages();

            return null;
        }
    }
}