// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserInfo.cs" company="Helpmebot Development Team">
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
//   Returns the user information about a specified user
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Castle.Core.Logging;

    using Helpmebot;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /* returns information about a user
    // what                 how                     info    message

    // contribs link        [calc]                  Done    Done
    // last contrib
    // userpage             [calc]                  Done    Done
    // usertalkpage         [calc]                  Done    Done
    // editcount            Commands.Count          Done    Done
    // registration date    Commands.Registration   Done    Done
    // block log            [calc]                  Done    Done
    // block status
    // user groups          Commands.Rights         Done    Done
    // editrate (edits/days) Commands.Age           Done    Done
    */
      
    /// <summary>
    ///   Returns the user information about a specified user
    /// </summary>
    internal class Userinfo : GenericCommand
    {
        /// <summary>
        /// The response.
        /// </summary>
        private readonly CommandResponseHandler response = new CommandResponseHandler();

        /// <summary>
        /// Initialises a new instance of the <see cref="Userinfo"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Userinfo(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The <see cref="CommandResponseHandler"/>.</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var args = this.Arguments;

            bool useLongInfo = bool.Parse(LegacyConfig.Singleton()["useLongUserInfo", this.Channel]);

            if (args.Length > 0)
            {
                if (args[0].ToLower() == "@long")
                {
                    useLongInfo = true;
                    GlobalFunctions.PopFromFront(ref args);
                }

                if (args[0].ToLower() == "@short")
                {
                    useLongInfo = false;
                    GlobalFunctions.PopFromFront(ref args);
                }
            }

            if (args.Length > 0)
            {
                string userName = string.Join(" ", args);

                UserInformation userInformation = new UserInformation
                                            {
                                                EditCount = Editcount.GetEditCount(userName, this.Channel)
                                            };

                if (userInformation.EditCount == -1)
                {
                    string[] mparams = { userName };
                    this.response.Respond(this.MessageService.RetrieveMessage("noSuchUser", this.Channel, mparams));
                    return this.response;
                }

                RetrieveUserInformation(userName, ref userInformation, this.Channel);

                if (useLongInfo)
                {
                    this.SendLongUserInfo(userInformation);
                }
                else
                {
                    this.SendShortUserInfo(userInformation);
                }
            }
            else
            {
                string[] messageParameters = { "userinfo", "1", args.Length.ToString(CultureInfo.InvariantCulture) };
                Helpmebot6.irc.IrcNotice(
                    this.Source.Nickname,
                    this.MessageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            return this.response;
        }

        /// <summary>
        /// Gets the user page URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>the user page url</returns>
        private static string GetUserPageUrl(string userName, string channel)
        {
            return Linker.GetRealLink(channel, "User:" + userName, true);
        }

        /// <summary>
        /// Gets the user talk page URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>the user talk page url</returns>
        private static string GetUserTalkPageUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            return Linker.GetRealLink(channel, "User_talk:" + userName, true);
        }

        /// <summary>
        /// Gets the user contributions URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>the contributions url</returns>
        private static string GetUserContributionsUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            
            return Linker.GetRealLink(channel, "Special:Contributions/" + userName, true);
        }

        /// <summary>
        /// Gets the block log URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>block log url</returns>
        private static string GetBlockLogUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            // replace mainpage in mainpage url with user:<username>
            userName = userName.Replace(" ", "_");

            return Linker.GetRealLink(channel, "Special:Log?type=block&page=User:" + userName, true);
        }

        // TODO: tidy up! why return a value when it's passed by ref anyway?

        /// <summary>
        /// Retrieves the user information.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="initial">The initial.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>the user info</returns>
// ReSharper disable UnusedMethodReturnValue.Local
        private static UserInformation RetrieveUserInformation(string userName, ref UserInformation initial, string channel)
// ReSharper restore UnusedMethodReturnValue.Local
        {
            try
            {
                initial.UserName = userName;

                if (initial.EditCount == 0)
                {
                    initial.EditCount = Editcount.GetEditCount(userName, channel);
                }

                initial.UserGroups = Rights.GetRights(userName, channel);

                initial.RegistrationDate = Registration.GetRegistrationDate(userName, channel);

                initial.UserPage = GetUserPageUrl(userName, channel);
                initial.TalkPage = GetUserTalkPageUrl(userName, channel);
                initial.UserContributions = GetUserContributionsUrl(userName, channel);
                initial.UserBlockLog = GetBlockLogUrl(userName, channel);

                initial.UserAge = Age.GetWikipedianAge(userName, channel);

                initial.EditRate = initial.EditCount / initial.UserAge.TotalDays;

                string baseWiki = LegacyConfig.Singleton()["baseWiki", channel];

                // FIXME: ServiceLocator
                var mediaWikiSiteRepository = ServiceLocator.Current.GetInstance<IMediaWikiSiteRepository>();
                MediaWikiSite mediaWikiSite = mediaWikiSiteRepository.GetById(int.Parse(baseWiki));
                BlockInformation bi = mediaWikiSite.GetBlockInformation(userName).FirstOrDefault();

                initial.BlockInformation = bi.Id ?? string.Empty;

                return initial;
            }
            catch (NullReferenceException ex)
            {
                ServiceLocator.Current.GetInstance<ILogger>().Error(ex.Message, ex);
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Sends the short user info.
        /// </summary>
        /// <param name="userInformation">The user information.</param>
        private void SendShortUserInfo(UserInformation userInformation)
        {
            const string Regex = "^http://en.wikipedia.org/wiki/";
            const string ShortUrlAlias = "http://enwp.org/";
            Regex r = new Regex(Regex);

            userInformation.UserPage = r.Replace(userInformation.UserPage, ShortUrlAlias);
            userInformation.TalkPage = r.Replace(userInformation.TalkPage, ShortUrlAlias);
            userInformation.UserBlockLog = r.Replace(userInformation.UserBlockLog, ShortUrlAlias);
            userInformation.UserContributions = r.Replace(userInformation.UserContributions, ShortUrlAlias);

            string[] messageParameters =
                {
                    userInformation.UserName, userInformation.UserPage, userInformation.TalkPage,
                    userInformation.UserContributions, userInformation.UserBlockLog,
                    userInformation.UserGroups, userInformation.UserAge.ToString(),
                    userInformation.RegistrationDate.ToString(CultureInfo.InvariantCulture),
                    userInformation.EditRate.ToString(CultureInfo.InvariantCulture), 
                    userInformation.EditCount.ToString(CultureInfo.InvariantCulture),
                    userInformation.BlockInformation == string.Empty ? string.Empty : "BLOCKED"
                };

            string message = this.MessageService.RetrieveMessage("cmdUserInfoShort", this.Channel, messageParameters);

            this.response.Respond(message);
        }

        /// <summary>
        /// Sends the long user info.
        /// </summary>
        /// <param name="userInformation">The user information.</param>
        private void SendLongUserInfo(UserInformation userInformation)
        {
            this.response.Respond(userInformation.UserPage);
            this.response.Respond(userInformation.TalkPage);
            this.response.Respond(userInformation.UserContributions);
            this.response.Respond(userInformation.UserBlockLog);
            this.response.Respond(userInformation.BlockInformation);
            string message;
            if (userInformation.UserGroups != string.Empty)
            {
                string[] messageParameters = { userInformation.UserName, userInformation.UserGroups };
                message = this.MessageService.RetrieveMessage("cmdRightsList", this.Channel, messageParameters);
            }
            else
            {
                string[] messageParameters = { userInformation.UserName };
                message = this.MessageService.RetrieveMessage("cmdRightsNone", this.Channel, messageParameters);
            }

            this.response.Respond(message);

            string[] messageParameters2 = { userInformation.EditCount.ToString(CultureInfo.InvariantCulture), userInformation.UserName };
            message = this.MessageService.RetrieveMessage("editCount", this.Channel, messageParameters2);
            this.response.Respond(message);

            string[] messageParameters3 =
                {
                    userInformation.UserName,
                    userInformation.RegistrationDate.ToString("hh:mm:ss t"),
                    userInformation.RegistrationDate.ToString("d MMMM yyyy")
                };
            message = this.MessageService.RetrieveMessage("registrationDate", this.Channel, messageParameters3);
            this.response.Respond(message);
            string[] messageParameters4 = { userInformation.UserName, userInformation.EditRate.ToString(CultureInfo.InvariantCulture) };
            message = this.MessageService.RetrieveMessage("editRate", this.Channel, messageParameters4);
            this.response.Respond(message);
        }

        /// <summary>
        /// Structure to hold the user info retrieved.
        /// </summary>
        private struct UserInformation
        {
            /// <summary>
            /// The user name.
            /// </summary>
            public string UserName;

            /// <summary>
            /// The edit count.
            /// </summary>
            public int EditCount;

            /// <summary>
            /// The user groups.
            /// </summary>
            public string UserGroups;

            /// <summary>
            /// The registration date.
            /// </summary>
            public DateTime RegistrationDate;

            /// <summary>
            /// The edit rate.
            /// </summary>
            public double EditRate;

            /// <summary>
            /// The user page.
            /// </summary>
            public string UserPage;

            /// <summary>
            /// The talk page.
            /// </summary>
            public string TalkPage;

            /// <summary>
            /// The user contributions.
            /// </summary>
            public string UserContributions;

            /// <summary>
            /// The user block log.
            /// </summary>
            public string UserBlockLog;

            /// <summary>
            /// The user age.
            /// </summary>
            public TimeSpan UserAge;

            /// <summary>
            /// The block information.
            /// </summary>
            public string BlockInformation;
        }
    }
}
