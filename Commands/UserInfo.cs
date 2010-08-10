#region Usings

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

#endregion

namespace helpmebot6.Commands
{
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

        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            bool useLongInfo =
                bool.Parse(Configuration.singleton().retrieveLocalStringOption("useLongUserInfo", channel));

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

                Editcount countCommand = new Editcount();
                uInfo.editCount = countCommand.getEditCount(userName, channel);

                if (uInfo.editCount == -1)
                {
                    string[] mparams = {userName};
                    this._crh.respond(Configuration.singleton().getMessage("noSuchUser", mparams));
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
                                         Configuration.singleton().getMessage("notEnoughParameters", messageParameters));
            }
            return this._crh;
        }

        private static string getUserPageUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            // look up site id
            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

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

        private static string getUserTalkPageUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            // look up site id
            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

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

        private static string getUserContributionsUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            // look up site id
            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

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

        private static string getBlockLogUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            // look up site id
            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

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
        private static UserInformation retrieveUserInformation(string userName, ref UserInformation initial, string channel)
// ReSharper restore UnusedMethodReturnValue.Local
        {
            try{
                initial.userName = userName;

                if (initial.editCount == 0)
                {
                    Editcount countCommand = new Editcount();
                    initial.editCount = countCommand.getEditCount(userName, channel);
                }

                Rights rightsCommand = new Rights();
                initial.userGroups = rightsCommand.getRights(userName, channel);

                Registration registrationCommand = new Registration();
                initial.registrationDate = registrationCommand.getRegistrationDate(userName, channel);

                initial.userPage = getUserPageUrl(userName, channel);
                initial.talkPage = getUserTalkPageUrl(userName, channel);
                initial.userContribs = getUserContributionsUrl(userName, channel);
                initial.userBlockLog = getBlockLogUrl(userName, channel);

                Age ageCommand = new Age();
                initial.userAge = ageCommand.getWikipedianAge(userName, channel);

                initial.editRate = initial.editCount/initial.userAge.TotalDays;

                initial.blockInformation = new Blockinfo().getBlockInformation(userName, channel).ToString();

                return initial;
            }
            catch (NullReferenceException ex)
            {
                GlobalFunctions.errorLog(ex);
                throw new InvalidOperationException();
            }
        }

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
                                             userInformation.editCount.ToString()
                                         };

            string message = Configuration.singleton().getMessage("cmdUserInfoShort", messageParameters);

            this._crh.respond(message);
        }

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
                message = Configuration.singleton().getMessage("cmdRightsList", messageParameters);
            }
            else
            {
                string[] messageParameters = {userInformation.userName};
                message = Configuration.singleton().getMessage("cmdRightsNone", messageParameters);
            }
            this._crh.respond(message);

            string[] messageParameters2 = {userInformation.editCount.ToString(), userInformation.userName};
            message = Configuration.singleton().getMessage("editCount", messageParameters2);
            this._crh.respond(message);

            string[] messageParameters3 = {
                                              userInformation.userName,
                                              userInformation.registrationDate.ToString("hh:mm:ss t"),
                                              userInformation.registrationDate.ToString("d MMMM yyyy")
                                          };
            message = Configuration.singleton().getMessage("registrationDate", messageParameters3);
            this._crh.respond(message);
            string[] messageParameters4 = {userInformation.userName, userInformation.editRate.ToString()};
            message = Configuration.singleton().getMessage("editRate", messageParameters4);
            this._crh.respond(message);
        }

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