#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Access : GenericCommand
    {
        protected override CommandResponseHandler accessDenied(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponseHandler crh = new Myaccess().run(source, channel, args);
            crh.append(base.accessDenied(source, channel, args));
            return crh;
        }

        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponseHandler crh = new CommandResponseHandler();
            if (args.Length > 1)
            {
                switch (args[0].ToLower())
                {
                    case "add":
                        if (args.Length > 2)
                        {
                            User.UserRights aL = User.UserRights.Normal;

                            switch (args[2].ToLower())
                            {
                                case "developer":
                                    aL = User.UserRights.Developer;
                                    break;
                                case "superuser":
                                    aL = User.UserRights.Superuser;
                                    break;
                                case "advanced":
                                    aL = User.UserRights.Advanced;
                                    break;
                                case "semi-ignored":
                                    aL = User.UserRights.Semiignored;
                                    break;
                                case "ignored":
                                    aL = User.UserRights.Ignored;
                                    break;
                                case "normal":
                                    aL = User.UserRights.Normal;
                                    break;
                                default:
                                    break;
                            }

                            crh = addAccessEntry(User.newFromString(args[1]), aL);
                        }
                        break;
                    case "del":
                        crh = delAccessEntry(int.Parse(args[1]));
                        break;
                }
                // add <source> <level>

                // del <id>
            }
            return crh;
        }

        private static CommandResponseHandler addAccessEntry(User newEntry, User.UserRights accessLevel)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string[] messageParams = {newEntry.ToString(), accessLevel.ToString()};
            string message = Configuration.singleton().getMessage("addAccessEntry", messageParams);

            // "Adding access entry for " + newEntry.ToString( ) + " at level " + accessLevel.ToString( )"
            Logger.instance().addToLog("Adding access entry for " + newEntry + " at level " + accessLevel,
                                       Logger.LogTypes.Command);
            DAL.singleton().insert("user", "", newEntry.nickname, newEntry.username, newEntry.hostname,
                                   accessLevel.ToString());

            return new CommandResponseHandler(message);
        }

        private static CommandResponseHandler delAccessEntry(int id)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string[] messageParams = {id.ToString()};
            string message = Configuration.singleton().getMessage("removeAccessEntry", messageParams);

            Logger.instance().addToLog("Removing access entry #" + id, Logger.LogTypes.Command);
            DAL.singleton().delete("user", 1, new DAL.WhereConds("user_id", id.ToString()));

            return new CommandResponseHandler(message);
        }
    }
}