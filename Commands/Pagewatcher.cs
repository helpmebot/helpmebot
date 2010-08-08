// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System.Reflection;
using helpmebot6.Monitoring.PageWatcher;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    /// Controls the page watcher thread
    /// </summary>
    internal class Pagewatcher : GenericCommand
    {
        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds a page watcher to a channel.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Removes a page watcher from a channel.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
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