// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JoinMessageServiceTests.cs" company="Helpmebot Development Team">
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
//   Defines the JoinMessageServiceTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.Services
{
    using System.Collections.Generic;
    using Helpmebot.ChannelServices.Configuration;
    using Helpmebot.ChannelServices.Services;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Model;
    using NSubstitute;
    using NUnit.Framework;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// The join message service tests.
    /// </summary>
    [TestFixture]
    public class JoinMessageServiceTests : TestBase
    {

        private WelcomeUser welcomeUser;
        private WelcomeUser ignoreUser;
        
        private JoinMessageService joinMessageService;
        
        private IBlockMonitoringService blockMonitoringService;
        private IIrcClient ircClient;
        private IGeolocationService geolocService;
        private IResponder responderMock;
        private IJoinMessageConfigurationService configService;
        
        public override void LocalSetup()
        {
            this.welcomeUser = new WelcomeUser { Host = "ab/.*", User = ".*", Nick = ".*", Account = ".*", RealName = ".*"};
            this.ignoreUser = new WelcomeUser { Host = ".*", User = "ign", Nick = ".*", Account = ".*", RealName = ".*", Exception = true };
        }

        /// <summary>
        /// The test setup.
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            this.ircClient = Substitute.For<IIrcClient>();
            this.geolocService = Substitute.For<IGeolocationService>();
            this.responderMock = Substitute.For<IResponder>();

            this.blockMonitoringService = Substitute.For<IBlockMonitoringService>();

            this.configService = Substitute.For<IJoinMessageConfigurationService>();
            
            this.joinMessageService = new JoinMessageService(
                this.Logger,
                this.responderMock,
                new ModuleConfiguration{JoinMessageRateLimits = new RateLimitConfiguration{RateLimitMax = 1, RateLimitDuration = 10}},
                this.geolocService,
                this.ircClient,
                this.blockMonitoringService,
                Substitute.For<IChannelManagementService>(),
                this.configService,
                Substitute.For<ICrossChannelService>()
                );

            this.configService.GetUsers("ab")
                .Returns(new List<WelcomeUser> { this.welcomeUser });
            this.configService.GetUsers("ef")
                .Returns(new List<WelcomeUser> { this.welcomeUser });
            this.configService.GetExceptions("ab")
                .Returns(new List<WelcomeUser> { this.ignoreUser });
            this.configService.GetExceptions("ef")
                .Returns(new List<WelcomeUser> { this.ignoreUser });
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldNotWelcomeUnknownUser()
        {
            // arrange
            var networkUser = Substitute.For<IUser>();

            networkUser.Nickname = "ab";
            networkUser.Username = "ab";
            networkUser.Hostname = "cd/test";

            this.joinMessageService.ClearRateLimitCache();

            var ea = new JoinEventArgs(null, networkUser, "ab", this.ircClient);
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);

            // assert
            this.ircClient.Received(0).SendMessage("ab", Arg.Any<string>());
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldNotWelcomeIgnoredUser()
        {
            // arrange
            var networkUser = Substitute.For<IUser>();

            networkUser.Nickname = "ab";
            networkUser.Username = "ign";
            networkUser.Hostname = "ab/test";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser, "ab", this.ircClient);
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);
            
            // assert
            this.ircClient.Received(0).SendMessage("ab", Arg.Any<string>());
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldWelcomeUser()
        {
            // arrange
            var networkUser = Substitute.For<IUser>();
            
            networkUser.Nickname = "ab";
            networkUser.Username = "ab";
            networkUser.Hostname = "ab/test";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser, "ab", this.ircClient);
            
            this.configService.GetOverride(Arg.Any<string>(), Arg.Any<string>()).Returns((WelcomerOverride)null);
            
            this.responderMock.Respond(
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<object[]>(),
                        null,
                        CommandResponseDestination.Default,
                        CommandResponseType.Message,
                        false,
                        null)
                .Returns(new[] { new CommandResponse { Message = "ab" } });
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);

            // assert
            this.ircClient.Received(1).SendMessage("ab", Arg.Any<string>());
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldNotWelcomeUserOnUnknownChannel()
        {
            // arrange
            this.configService.GetUsers("cd")
                .Returns(new List<WelcomeUser>());
            this.configService.GetExceptions("cd")
                .Returns(new List<WelcomeUser>());

            var networkUser = Substitute.For<IUser>();

            networkUser.Nickname = "ab";
            networkUser.Username = "ab";
            networkUser.Hostname = "ab/test";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser, "cd", this.ircClient);

            // act
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);

            // assert
            this.ircClient.Received(0).SendMessage("cd", Arg.Any<string>());
            this.ircClient.Received(0).SendMessage("ab", Arg.Any<string>());
        }

        /// <summary>
        /// The should rate limit.
        /// </summary>
        [Test]
        public void ShouldRateLimitByHostname()
        {
            var networkUser = Substitute.For<IUser>();
            networkUser.Nickname = "ab";
            networkUser.Username = "ab";
            networkUser.Hostname = "ab/test";

            var networkUser2 = Substitute.For<IUser>();
            networkUser2.Nickname = "ab";
            networkUser2.Username = "ab2";
            networkUser2.Hostname = "ab/test2";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser, "ab", this.ircClient);
            var ea2 = new JoinEventArgs(null, networkUser2, "ab", this.ircClient);

            this.configService.GetOverride(Arg.Any<string>(), Arg.Any<string>()).Returns((WelcomerOverride)null);
            
            this.responderMock.Respond(
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<object[]>(),
                        null,
                        CommandResponseDestination.Default,
                        CommandResponseType.Message,
                        false,
                        null)
                .Returns(new[] { new CommandResponse { Message = "ab" } });
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea2);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);

            // assert
            this.ircClient.Received(2).SendMessage("ab", Arg.Any<string>());
        }

        /// <summary>
        /// The should rate limit.
        /// </summary>
        [Test]
        public void ShouldRateLimit()
        {
            var networkUser = Substitute.For<IUser>();
            networkUser.Nickname = "ab";
            networkUser.Username = "ab";
            networkUser.Hostname = "ab/test";

            this.responderMock.Respond(
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<object[]>(),
                        null,
                        CommandResponseDestination.Default,
                        CommandResponseType.Message,
                        false,
                        null)
                .Returns(new[] { new CommandResponse { Message = "ab" } });

            this.configService.GetOverride(Arg.Any<string>(), Arg.Any<string>()).Returns((WelcomerOverride) null);
            
            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser, "ab", this.ircClient);

            // act
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);

            // assert
            this.ircClient.Received(1).SendMessage("ab", Arg.Any<string>());
        }

        /// <summary>
        /// The should rate limit.
        /// </summary>
        [Test]
        public void ShouldRateLimitByChannel()
        {
            var networkUser = Substitute.For<IUser>();
            networkUser.Nickname = "ab";
            networkUser.Username = "ab";
            networkUser.Hostname = "ab/test";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser, "ab", this.ircClient);
            var ea2 = new JoinEventArgs(null, networkUser, "ef", this.ircClient);

            this.configService.GetOverride(Arg.Any<string>(), Arg.Any<string>()).Returns((WelcomerOverride) null);

            this.responderMock.Respond(
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<object[]>(),
                        null,
                        CommandResponseDestination.Default,
                        CommandResponseType.Message,
                        false,
                        null)
                .Returns(new[] { new CommandResponse { Message = "ab" } });
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea2);
            this.joinMessageService.OnJoinEvent(this.ircClient, ea2);

            // assert
            this.ircClient.Received(1).SendMessage("ab", Arg.Any<string>());
            this.ircClient.Received(1).SendMessage("ef", Arg.Any<string>());
        }
    }
}
