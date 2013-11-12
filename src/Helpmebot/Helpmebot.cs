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

namespace Helpmebot
{
    using System;
    using System.Runtime.CompilerServices;

    using Castle.Core.Logging;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    using Helpmebot.AI;
    using Helpmebot.IRC.Events;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.IRC;
    using Helpmebot.Monitoring;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Startup;
    using Helpmebot.Threading;

    using helpmebot6.Commands;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Helpmebot main class
    /// </summary>
    public class Helpmebot6
    {
        private static IWindsorContainer container;

        public static IrcAccessLayer irc;
        private static DAL _dbal;

        public static string debugChannel;
        public static string mainChannel;

        private static uint _ircNetwork;

        public static readonly DateTime StartupTime = DateTime.Now;

        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        private static void Main(string[] args)
        {
            BootstrapContainer();

            ServiceLocator.Current.GetInstance<ILogger>().Info("Initialising Helpmebot...");

            InitialiseBot();
        }

        /// <summary>
        /// The bootstrap container.
        /// </summary>
        private static void BootstrapContainer()
        {
            container = new WindsorContainer();

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            container.Install(FromAssembly.This(new WindsorBootstrap()));
        }

        /// <summary>
        /// The initialise bot.
        /// </summary>
        private static void InitialiseBot()
        {
            _dbal = DAL.singleton();

            if (!_dbal.connect())
            {
                // can't connect to database, DIE
                return;
            }

            LegacyConfig.singleton();

            debugChannel = LegacyConfig.singleton()["channelDebug"];

            _ircNetwork = uint.Parse(LegacyConfig.singleton()["ircNetwork"]);

            Trigger = LegacyConfig.singleton()["commandTrigger"];

            irc = new IrcAccessLayer(_ircNetwork);

            SetupEvents();

            if (!irc.Connect())
            {
                // if can't connect to irc, die
                return;
            }

            new MonitorService(62167, "Helpmebot v6 (Nagios Monitor service)");

            // ACC notification monitor
            AccNotifications.getInstance();
        }


        private static void SetupEvents()
        {
            irc.connectionRegistrationSucceededEvent += JoinChannels;

            irc.joinEvent += welcomeNewbieOnJoinEvent;

            irc.joinEvent += NotifyOnJoinEvent;

            irc.PrivateMessageEvent += ReceivedMessage;

            irc.inviteEvent += irc_InviteEvent;

            irc.ThreadFatalErrorEvent += IrcThreadFatalErrorEvent;
        }

        private static void IrcThreadFatalErrorEvent(object sender, EventArgs e)
        {
            Stop();
        }

        private static void irc_InviteEvent(User source, string nickname, string channel)
        {
            // FIXME: Remove service locator!
            new Join(source, nickname, new[] { channel }, ServiceLocator.Current.GetInstance<IMessageService>()).RunCommand();
        }

        private static void welcomeNewbieOnJoinEvent(User source, string channel)
        {
            NewbieWelcomer.Instance().Execute(source, channel);
        }

        private static void NotifyOnJoinEvent(User source, string channel)
        {
            // FIXME: Remove service locator!
            new Notify(source, channel, new string[0], ServiceLocator.Current.GetInstance<IMessageService>()).NotifyJoin(source, channel);
        }

        /// <summary>
        /// The received message.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void ReceivedMessage(object sender, PrivateMessageEventArgs e)
        {
            string message = e.Message;

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

                    cmd.handleCommand(e.Sender, e.Destination, command, commandArgs);
                }

                string aiResponse = Intelligence.Singleton().Respond(message);
                if (LegacyConfig.singleton()["silence", e.Destination] == "false" && aiResponse != string.Empty)
                {
                    string[] aiParameters = { e.Sender.nickname };
                    irc.IrcPrivmsg(e.Destination, new Message().GetMessage(aiResponse, aiParameters));
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.Current.GetInstance<ILogger>().Error(ex.Message, ex);
            }
        }

        private static void JoinChannels(object sender, EventArgs e)
        {
            irc.IrcJoin(debugChannel);

            DAL.Select q = new DAL.Select("channel_name");
            q.setFrom("channel");
            q.addWhere(new DAL.WhereConds("channel_enabled", 1));
            q.addWhere(new DAL.WhereConds("channel_network", _ircNetwork.ToString()));
            foreach (object[] item in _dbal.executeSelect(q))
            {
                irc.IrcJoin((string) (item)[0]);
            }
        }

        public static void Stop()
        {
            ThreadList.instance().stop();
        }

        /// <summary>
        /// Gets or sets the trigger.
        /// </summary>
        public static string Trigger { get; set; }
    }
}