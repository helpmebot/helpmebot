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

    using helpmebot6.Model;

    // returns information about a user
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


    /// <summary>
    ///   Returns the user information about a specified user
    /// </summary>
    internal class Userinfo : GenericCommand
    {
        private readonly CommandResponseHandler _crh = new CommandResponseHandler();

        public Userinfo(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            bool useLongInfo =
                bool.Parse(Configuration.singleton()["useLongUserInfo",channel]);

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

                UserInformation uInfo = new UserInformation();

                uInfo.editCount = Editcount.GetEditCount(userName, channel);

                if (uInfo.editCount == -1)
                {
                    string[] mparams = {userName};
                    this._crh.respond(new Message().get("noSuchUser", mparams));
                    return this._crh;
                }

                retrieveUserInformation(userName, ref uInfo, channel);


                //##################################################


                if (useLongInfo)
                {
                    sendLongUserInfo(uInfo);
                }
                else
                {
                    sendShortUserInfo(uInfo);
                }
            }
            else
            {
                string[] messageParameters = {"userinfo", "1", args.Length.ToString()};
                Helpmebot6.irc.ircNotice(source.nickname,
                                         new Message().get("notEnoughParameters", messageParameters));
            }
            return this._crh;
        }

        /// <summary>
        /// Gets the user page URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        private static string getUserPageUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            // look up site id
            string baseWiki = Configuration.singleton()["baseWiki",channel];

            // get api
            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);


            // api-> get mainpage name (Mediawiki:mainpage)
            const string apiQuery = "?action=query&prop=revisions&titles=Mediawiki:Mainpage&rvprop=content&format=xml";
            XmlTextReader creader = new XmlTextReader(HttpRequest.get(api + apiQuery));
            do
            {
                creader.Read();
            } while (creader.Name != "rev");
            string mainpagename = creader.ReadElementContentAsString();

            mainpagename = mainpagename.Replace(" ", "_");

            // get mainpage url from site table
            q = new DAL.Select("site_mainpage");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string mainpageurl = DAL.singleton().executeScalarSelect(q);

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace(" ", "_");

            return mainpageurl.Replace(mainpagename, "User:" + userName);
        }

        /// <summary>
        /// Gets the user talk page URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        private static string getUserTalkPageUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            // look up site id
            string baseWiki = Configuration.singleton()["baseWiki",channel];

            // get api
            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            // api-> get mainpage name (Mediawiki:mainpage)
            const string apiQuery = "?action=query&prop=revisions&titles=Mediawiki:Mainpage&rvprop=content&format=xml";
            XmlTextReader creader = new XmlTextReader(HttpRequest.get(api + apiQuery));
            do
            {
                creader.Read();
            } while (creader.Name != "rev");
            string mainpagename = creader.ReadElementContentAsString();

            mainpagename = mainpagename.Replace(" ", "_");

            // get mainpage url from site table
            q = new DAL.Select("site_mainpage");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string mainpageurl = DAL.singleton().executeScalarSelect(q);

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace(" ", "_");

            return mainpageurl.Replace(mainpagename, "User_talk:" + userName);
        }

        /// <summary>
        /// Gets the user contributions URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        private static string getUserContributionsUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            // look up site id
            string baseWiki = Configuration.singleton()["baseWiki",channel];

            // get api
            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            // api-> get mainpage name (Mediawiki:mainpage)
            const string apiQuery = "?action=query&prop=revisions&titles=Mediawiki:Mainpage&rvprop=content&format=xml";
            XmlTextReader creader = new XmlTextReader(HttpRequest.get(api + apiQuery));
            do
            {
                creader.Read();
            } while (creader.Name != "rev");
            string mainpagename = creader.ReadElementContentAsString();

            mainpagename = mainpagename.Replace(" ", "_");

            // get mainpage url from site table
            q = new DAL.Select("site_mainpage");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string mainpageurl = DAL.singleton().executeScalarSelect(q);

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace(" ", "_");

            return mainpageurl.Replace(mainpagename, "Special:Contributions/" + userName);
        }

        /// <summary>
        /// Gets the block log URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        private static string getBlockLogUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            // look up site id
            string baseWiki = Configuration.singleton()["baseWiki",channel];

            // get api
            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            // api-> get mainpage name (Mediawiki:mainpage)
            const string apiQuery = "?action=query&prop=revisions&titles=Mediawiki:Mainpage&rvprop=content&format=xml";
            XmlTextReader creader = new XmlTextReader(HttpRequest.get(api + apiQuery));
            do
            {
                creader.Read();
            } while (creader.Name != "rev");
            string mainpagename = creader.ReadElementContentAsString();

            mainpagename = mainpagename.Replace(" ", "_");

            // get mainpage url from site table
            q = new DAL.Select("site_mainpage");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string mainpageurl = DAL.singleton().executeScalarSelect(q);

            // replace mainpage in mainpage url with user:<username>

            userName = userName.Replace(" ", "_");

            return mainpageurl.Replace(mainpagename, "Special:Log?type=block&page=User:" + userName);
        }

        //TODO: tidy up! why return a value when it's passed by ref anyway?
// ReSharper disable UnusedMethodReturnValue.Local
        /// <summary>
        /// Retrieves the user information.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="initial">The initial.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        private static UserInformation retrieveUserInformation(string userName, ref UserInformation initial, string channel)
// ReSharper restore UnusedMethodReturnValue.Local
        {
            try
            {
                initial.userName = userName;

                if (initial.editCount == 0)
                {
                    initial.editCount = Editcount.GetEditCount(userName, channel);
                }

                initial.userGroups = Rights.GetRights(userName, channel);

                initial.registrationDate = Registration.GetRegistrationDate(userName, channel);

                initial.userPage = getUserPageUrl(userName, channel);
                initial.talkPage = getUserTalkPageUrl(userName, channel);
                initial.userContribs = getUserContributionsUrl(userName, channel);
                initial.userBlockLog = getBlockLogUrl(userName, channel);

                initial.userAge = Age.GetWikipedianAge(userName, channel);

                initial.editRate = initial.editCount / initial.userAge.TotalDays;

                BlockInformation bi = Blockinfo.GetBlockInformation(userName, channel);
                if (bi.Id == null)
                {
                    initial.blockInformation = string.Empty;
                }
                else
                {
                    initial.blockInformation = bi.Id.ToString();
                }

                return initial;
            }
            catch (NullReferenceException ex)
            {
                GlobalFunctions.errorLog(ex);
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Sends the short user info.
        /// </summary>
        /// <param name="userInformation">The user information.</param>
        private void sendShortUserInfo(UserInformation userInformation)
        {
            const string regex = "^http://en.wikipedia.org/wiki/";
            const string shortUrlAlias = "http://enwp.org/";
            Regex r = new Regex(regex);

            userInformation.userPage = r.Replace(userInformation.userPage, shortUrlAlias);
            userInformation.talkPage = r.Replace(userInformation.talkPage, shortUrlAlias);
            userInformation.userBlockLog = r.Replace(userInformation.userBlockLog, shortUrlAlias);
            userInformation.userContribs = r.Replace(userInformation.userContribs, shortUrlAlias);

            string[] messageParameters = {
                                             userInformation.userName,
                                             userInformation.userPage,
                                             userInformation.talkPage,
                                             userInformation.userContribs,
                                             userInformation.userBlockLog,
                                             userInformation.userGroups,
                                             userInformation.userAge.ToString(),
                                             userInformation.registrationDate.ToString(),
                                             userInformation.editRate.ToString(),
                                             userInformation.editCount.ToString(),
                                             userInformation.blockInformation == "" ? "" : "BLOCKED"
                                         };

            string message = new Message().get("cmdUserInfoShort", messageParameters);

            this._crh.respond(message);
        }

        /// <summary>
        /// Sends the long user info.
        /// </summary>
        /// <param name="userInformation">The user information.</param>
        private void sendLongUserInfo(UserInformation userInformation)
        {
            this._crh.respond(userInformation.userPage);
            this._crh.respond(userInformation.talkPage);
            this._crh.respond(userInformation.userContribs);
            this._crh.respond(userInformation.userBlockLog);
            this._crh.respond(userInformation.blockInformation);
            string message;
            if (userInformation.userGroups != "")
            {
                string[] messageParameters = {userInformation.userName, userInformation.userGroups};
                message = new Message().get("cmdRightsList", messageParameters);
            }
            else
            {
                string[] messageParameters = {userInformation.userName};
                message = new Message().get("cmdRightsNone", messageParameters);
            }
            this._crh.respond(message);

            string[] messageParameters2 = {userInformation.editCount.ToString(), userInformation.userName};
            message = new Message().get("editCount", messageParameters2);
            this._crh.respond(message);

            string[] messageParameters3 = {
                                              userInformation.userName,
                                              userInformation.registrationDate.ToString("hh:mm:ss t"),
                                              userInformation.registrationDate.ToString("d MMMM yyyy")
                                          };
            message = new Message().get("registrationDate", messageParameters3);
            this._crh.respond(message);
            string[] messageParameters4 = {userInformation.userName, userInformation.editRate.ToString()};
            message = new Message().get("editRate", messageParameters4);
            this._crh.respond(message);
        }

        /// <summary>
        /// Structure to hold the userinfo retrieved.
        /// </summary>
        private struct UserInformation
        {
            public string userName;
            public int editCount;
            public string userGroups;
            public DateTime registrationDate;
            public double editRate;
            public string userPage;
            public string talkPage;
            public string userContribs;
            public string userBlockLog;
            public TimeSpan userAge;
            public string blockInformation;
        }
    }
}