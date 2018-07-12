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
    using System.Web;

    using Castle.Core.Logging;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
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

        private readonly ILinkerService linker;
        
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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Userinfo(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {   
            // fixme: servicelocator
            this.linker = ServiceLocator.Current.GetInstance<ILinkerService>();
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The <see cref="CommandResponseHandler"/>.</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var args = this.Arguments;

            var messageService = this.CommandServiceHelper.MessageService;
            if (args.Length > 0)
            {
                string userName = string.Join(" ", args);

                try
                {
                    var userInformation = this.RetrieveUserInformation(userName);

                    this.SendShortUserInfo(userInformation);
                }
                catch (MediawikiApiException)
                {
                    string[] mparams = { userName };
                    this.response.Respond(messageService.RetrieveMessage("noSuchUser", this.Channel, mparams));
                    return this.response;
                }
            }
            else
            {
                string[] messageParameters = { "userinfo", "1", args.Length.ToString(CultureInfo.InvariantCulture) };

                var notEnoughParamsMessage = messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters);
                this.CommandServiceHelper.Client.SendNotice(this.Source.Nickname, notEnoughParamsMessage);
            }

            return this.response;
        }

        /// <summary>
        /// Gets the user page URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>the user page url</returns>
        private string GetUserPageUrl(string userName, string channel)
        {
            return this.linker.ConvertWikilinkToUrl(channel, "User:" + userName);
        }

        /// <summary>
        /// Gets the user talk page URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>the user talk page url</returns>
        private string GetUserTalkPageUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            return this.linker.ConvertWikilinkToUrl(channel, "User_talk:" + userName);
        }

        /// <summary>
        /// Gets the user contributions URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>the contributions url</returns>
        private string GetUserContributionsUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }
            
            return this.linker.ConvertWikilinkToUrl(channel, "Special:Contributions/" + userName);
        }

        /// <summary>
        /// Gets the block log URL.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>block log url</returns>
        private string GetBlockLogUrl(string userName, string channel)
        {
            if (userName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            // replace mainpage in mainpage url with user:<username>
            userName = userName.Replace(" ", "_");

            var blockLogUrl = new UriBuilder(this.linker.ConvertWikilinkToUrl(channel, "Special:Log"));

            var queryParts = HttpUtility.ParseQueryString(blockLogUrl.Query);
            queryParts["type"] = "block";
            queryParts["page"] = "User:" + userName;
            blockLogUrl.Query = queryParts.ToString();

            return blockLogUrl.Uri.ToString();
        }

        private UserInformation RetrieveUserInformation(string userName)
        {
            var initial = new UserInformation();
            
            try
            {
                var channelRepository = this.CommandServiceHelper.ChannelRepository;
                var channelObj = channelRepository.GetByName(this.Channel);
                var mediaWikiSite = this.CommandServiceHelper.MediaWikiSiteRepository.GetById(channelObj.BaseWiki);
            
                initial.UserName = userName;

                initial.EditCount = mediaWikiSite.GetEditCount(userName);
                initial.UserGroups = mediaWikiSite.GetRights(userName);
                
                var registrationDate = mediaWikiSite.GetRegistrationDate(userName);
                initial.RegistrationDate = registrationDate;
                initial.UserAge = registrationDate.HasValue
                    ? DateTime.Now.Subtract(registrationDate.Value)
                    : (TimeSpan?) null;
                
                initial.UserPage = this.GetUserPageUrl(userName, this.Channel);
                initial.TalkPage = this.GetUserTalkPageUrl(userName, this.Channel);
                initial.UserContributions = this.GetUserContributionsUrl(userName, this.Channel);
                initial.UserBlockLog = this.GetBlockLogUrl(userName, this.Channel);

                initial.EditRate = initial.UserAge.HasValue
                    ? initial.EditCount / initial.UserAge.Value.TotalDays
                    : (double?) null;

                var blockInfo = mediaWikiSite.GetBlockInformation(userName).FirstOrDefault();

                initial.BlockInformation = blockInfo.Id ?? string.Empty;

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
            const string Regex = "^https://en.wikipedia.org/wiki/";
            const string ShortUrlAlias = "http://enwp.org/";
            var r = new Regex(Regex);

            userInformation.UserPage = r.Replace(userInformation.UserPage, ShortUrlAlias);
            userInformation.TalkPage = r.Replace(userInformation.TalkPage, ShortUrlAlias);
            userInformation.UserBlockLog = r.Replace(userInformation.UserBlockLog, ShortUrlAlias);
            userInformation.UserContributions = r.Replace(userInformation.UserContributions, ShortUrlAlias);

            var urlShorteningService = this.CommandServiceHelper.UrlShorteningService;

            var age = "(N/A)";
            if (userInformation.UserAge.HasValue)
            {
                age = string.Format(
                    "{0}y {1}d {2}h {3}m",
                    (userInformation.UserAge.Value.Days / 365).ToString(CultureInfo.InvariantCulture),
                    (userInformation.UserAge.Value.Days % 365).ToString(CultureInfo.InvariantCulture),
                    userInformation.UserAge.Value.Hours.ToString(CultureInfo.InvariantCulture),
                    userInformation.UserAge.Value.Minutes.ToString(CultureInfo.InvariantCulture));
            }

            var regDate = "(N/A)";
            if (userInformation.RegistrationDate.HasValue)
            {
                regDate = userInformation.RegistrationDate.Value.ToString("u");
            }

            var editRate = "(N/A)";
            if (userInformation.EditRate.HasValue)
            {
                editRate = Math.Round(userInformation.EditRate.Value, 3).ToString(CultureInfo.InvariantCulture);
            }

            string[] messageParameters =
                {
                    userInformation.UserName,
                    urlShorteningService.Shorten(userInformation.UserPage),
                    urlShorteningService.Shorten(userInformation.TalkPage),
                    urlShorteningService.Shorten(userInformation.UserContributions),
                    urlShorteningService.Shorten(userInformation.UserBlockLog),
                    userInformation.UserGroups,
                    age,
                    regDate,
                    editRate,
                    userInformation.EditCount.ToString(CultureInfo.InvariantCulture),
                    userInformation.BlockInformation == string.Empty ? string.Empty : "| BLOCKED"
                };

            string message = this.CommandServiceHelper.MessageService.RetrieveMessage("cmdUserInfoShort", this.Channel, messageParameters);

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
            public DateTime? RegistrationDate;

            /// <summary>
            /// The edit rate.
            /// </summary>
            public double? EditRate;

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
            public TimeSpan? UserAge;

            /// <summary>
            /// The block information.
            /// </summary>
            public string BlockInformation;
        }
    }
}
