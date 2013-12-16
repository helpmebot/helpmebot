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
    using System.Text.RegularExpressions;
    using System.Xml;

    using Castle.Core.Logging;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using User = Helpmebot.User;
    

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
        public Userinfo(User source, string channel, string[] args, IMessageService messageService)
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

            bool useLongInfo = bool.Parse(LegacyConfig.singleton()["useLongUserInfo", this.Channel]);

            if (args.Length > 0)
            {
                if (args[0].ToLower() == "@long")
                {
                    useLongInfo = true;
                    GlobalFunctions.popFromFront(ref args);
                }

                if (args[0].ToLower() == "@short")
                {
                    useLongInfo = false;
                    GlobalFunctions.popFromFront(ref args);
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
                    this.response.respond(this.MessageService.RetrieveMessage("noSuchUser", this.Channel, mparams));
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
                string[] messageParameters = { "userinfo", "1", args.Length.ToString() };
                Helpmebot6.irc.IrcNotice(
                    this.Source.nickname,
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
        /// TODO: refactor me!
        private static string GetUserPageUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            // look up site id
            string baseWiki = LegacyConfig.singleton()["baseWiki", channel];

            var mainpageurl = GetMainPageUrl(baseWiki);

            // replace mainpage in mainpage url with user:<username>
            userName = userName.Replace(" ", "_");

            var mainpagename = GetMainPageName(baseWiki);
            return mainpageurl.Replace(mainpagename, "User:" + userName);
        }

        /// <summary>
        /// The get main page url.
        /// </summary>
        /// <param name="baseWiki">
        /// The base wiki.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetMainPageUrl(string baseWiki)
        {
            // get api
            DAL.Select q;

            // get mainpage url from site table
            q = new DAL.Select("site_mainpage");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string mainpageurl = DAL.singleton().executeScalarSelect(q);
            return mainpageurl;
        }

        /// <summary>
        /// Gets the user talk page URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>the user talk page url</returns>
        /// TODO: refactor me!
        private static string GetUserTalkPageUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            // look up site id
            string baseWiki = LegacyConfig.singleton()["baseWiki", channel];
            
            var mainpagename = GetMainPageName(baseWiki);

            var mainpageurl = GetMainPageUrl(baseWiki);

            // replace mainpage in mainpage url with user:<username>
            userName = userName.Replace(" ", "_");

            return mainpageurl.Replace(mainpagename, "User_talk:" + userName);
        }

        /// <summary>
        /// Gets the user contributions URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>the contributions url</returns>
        /// TODO: refactor me!
        private static string GetUserContributionsUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            // look up site id
            string baseWiki = LegacyConfig.singleton()["baseWiki", channel];

            var mainpagename = GetMainPageName(baseWiki);

            var mainpageurl = GetMainPageUrl(baseWiki);

            // replace mainpage in mainpage url with user:<username>
            userName = userName.Replace(" ", "_");

            return mainpageurl.Replace(mainpagename, "Special:Contributions/" + userName);
        }

        /// <summary>
        /// The get main page name.
        /// </summary>
        /// <param name="baseWiki">
        /// The base wiki.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetMainPageName(string baseWiki)
        {
            // get api
            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            // api-> get mainpage name (Mediawiki:mainpage)
            const string ApiQuery = "?action=query&prop=revisions&titles=Mediawiki:Mainpage&rvprop=content&format=xml";
            XmlTextReader creader = new XmlTextReader(HttpRequest.get(api + ApiQuery));
            do
            {
                creader.Read();
            }
            while (creader.Name != "rev");

            string mainpagename = creader.ReadElementContentAsString();

            mainpagename = mainpagename.Replace(" ", "_");
            return mainpagename;
        }

        /// <summary>
        /// Gets the block log URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>block log url</returns>
        /// TODO: refactor me!
        private static string GetBlockLogUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            // look up site id
            string baseWiki = LegacyConfig.singleton()["baseWiki", channel];

            var mainpagename = GetMainPageName(baseWiki);
            var mainpageurl = GetMainPageUrl(baseWiki);

            // replace mainpage in mainpage url with user:<username>
            userName = userName.Replace(" ", "_");

            return mainpageurl.Replace(mainpagename, "Special:Log?type=block&page=User:" + userName);
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

                BlockInformation bi = Blockinfo.GetBlockInformation(userName, channel);

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
                    userInformation.RegistrationDate.ToString(),
                    userInformation.EditRate.ToString(), userInformation.EditCount.ToString(),
                    userInformation.BlockInformation == string.Empty ? string.Empty : "BLOCKED"
                };

            string message = this.MessageService.RetrieveMessage("cmdUserInfoShort", this.Channel, messageParameters);

            this.response.respond(message);
        }

        /// <summary>
        /// Sends the long user info.
        /// </summary>
        /// <param name="userInformation">The user information.</param>
        private void SendLongUserInfo(UserInformation userInformation)
        {
            this.response.respond(userInformation.UserPage);
            this.response.respond(userInformation.TalkPage);
            this.response.respond(userInformation.UserContributions);
            this.response.respond(userInformation.UserBlockLog);
            this.response.respond(userInformation.BlockInformation);
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

            this.response.respond(message);

            string[] messageParameters2 = { userInformation.EditCount.ToString(), userInformation.UserName };
            message = this.MessageService.RetrieveMessage("editCount", this.Channel, messageParameters2);
            this.response.respond(message);

            string[] messageParameters3 =
                {
                    userInformation.UserName,
                    userInformation.RegistrationDate.ToString("hh:mm:ss t"),
                    userInformation.RegistrationDate.ToString("d MMMM yyyy")
                };
            message = this.MessageService.RetrieveMessage("registrationDate", this.Channel, messageParameters3);
            this.response.respond(message);
            string[] messageParameters4 = { userInformation.UserName, userInformation.EditRate.ToString() };
            message = this.MessageService.RetrieveMessage("editRate", this.Channel, messageParameters4);
            this.response.respond(message);
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
