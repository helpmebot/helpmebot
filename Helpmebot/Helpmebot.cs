// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helpmebot.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Helpmebot main class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6
{
    using System;

    using helpmebot6.AI;
    using helpmebot6.Commands;
    using helpmebot6.ExtensionMethods;
    using helpmebot6.Monitoring;
    using helpmebot6.Threading;

    /// <summary>
    /// Helpmebot main class
    /// </summary>
    public class Helpmebot6
    {
        public static IAL irc;
        private static DAL _dbal;
        
        private static string _trigger;

        public static string debugChannel;
        public static string mainChannel;

        private static uint _ircNetwork;

        public static readonly DateTime StartupTime = DateTime.Now;

        public static bool pagewatcherEnabled = true;
        
        private static void Main(string[] args)
        {
            // startup arguments
            int? configFileArg = args.ContainsPrefix("--configfile");
            string configFile = ".hmbot";
            if (configFileArg.HasValue)
            {
                configFile = args[configFileArg.Value].Substring(args[configFileArg.Value].IndexOf('='));
            }

            if (args.ContainsPrefix("--logdal").HasValue)
            {
                Logger.instance().logDAL = true;
            }

            if (args.ContainsPrefix("--logdallock").HasValue)
            {
                Logger.instance().logDalLock = true;
            }

            if (args.ContainsPrefix("--logirc").HasValue)
            {
                Logger.instance().logIrc = true;
            }

            if (args.ContainsPrefix("--disablepagewatcher").HasValue)
            {
                pagewatcherEnabled = false;
            }

            initialiseBot(configFile);
        }

        private static void initialiseBot(string configFile)
        {
            string username;
            string password;
            string schema;
            uint port = 0;
            string server = username = password = schema = "";

            Configuration.readHmbotConfigFile(configFile, ref server, ref username, ref password, ref port, ref schema);

            _dbal = DAL.singleton(server, port, username, password, schema);

            if (!_dbal.connect())
            {
                // can't connect to database, DIE
                return;
            }

            Configuration.singleton();

            debugChannel = Configuration.singleton()["channelDebug"];

            _ircNetwork = uint.Parse(Configuration.singleton()["ircNetwork"]);

            _trigger = Configuration.singleton()["commandTrigger"];

            irc = new IAL(_ircNetwork);

            new IrcProxy(irc, int.Parse(Configuration.singleton()["proxyPort"]), Configuration.singleton()["proxyPassword"]);

            setupEvents();

            if (!irc.connect())
            {
                // if can't connect to irc, die
                return;
            }

            new MonitorService(62167, "Helpmebot v6 (Nagios Monitor service)");

            // ACC notification monitor
            AccNotifications.getInstance();
        }


        private static void setupEvents()
        {
            irc.connectionRegistrationSucceededEvent += joinChannels;

            irc.joinEvent += welcomeNewbieOnJoinEvent;

            irc.joinEvent += notifyOnJoinEvent;

            irc.privmsgEvent += receivedMessage;

            irc.inviteEvent += irc_InviteEvent;

            irc.threadFatalError += irc_ThreadFatalError;
        }

        private static void irc_ThreadFatalError(object sender, EventArgs e)
        {
            stop();
        }

        private static void irc_InviteEvent(User source, string nickname, string channel)
        {
            new Join(source, nickname, new[] {channel}).RunCommand();
        }

        private static void welcomeNewbieOnJoinEvent(User source, string channel)
        {
            NewbieWelcomer.instance().execute(source, channel);
        }

        private static void notifyOnJoinEvent(User source, string channel)
        {
            new Notify(source, channel, new string[0]).NotifyJoin(source, channel);
        }

        private static void receivedMessage(User source, string destination, string message)
        {
            CommandParser cmd = new CommandParser();
            try
            {
                bool overrideSilence = cmd.OverrideBotSilence;
                if (CommandParser.isRecognisedMessage(ref message, ref overrideSilence))
                {
                    cmd.OverrideBotSilence = overrideSilence;
                    string[] messageWords = message.Split(' ');
                    string command = messageWords[0];
                    string joinedargs = string.Join(" ", messageWords, 1, messageWords.Length - 1);
                    string[] commandArgs = joinedargs == string.Empty ? new string[0] : joinedargs.Split(' ');

                    cmd.handleCommand(source, destination, command, commandArgs);
                }
                string aiResponse = Intelligence.Singleton().Respond(message);
                if (Configuration.singleton()["silence",destination] == "false" &&
                    aiResponse != string.Empty)
                {
                    string[] aiParameters = {source.nickname};
                    irc.ircPrivmsg(destination, new Message().get(aiResponse, aiParameters));
                }
            }
            catch (Exception ex)
            {
                GlobalFunctions.errorLog(ex);
            }
        }

        private static void joinChannels()
        {
            irc.ircJoin(debugChannel);

            DAL.Select q = new DAL.Select("channel_name");
            q.setFrom("channel");
            q.addWhere(new DAL.WhereConds("channel_enabled", 1));
            q.addWhere(new DAL.WhereConds("channel_network", _ircNetwork.ToString()));
            foreach (object[] item in _dbal.executeSelect(q))
            {
                irc.ircJoin((string) (item)[0]);
            }
        }

        public static void stop()
        {
            ThreadList.instance().stop();
        }

        public static string trigger
        {
            get
            {
                return _trigger;
            }
            set
            {
                _trigger = value;
            }
        }
    }
}