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
    using Moq;

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
        
        private Mock<IBlockMonitoringService> blockMonitoringService;
        private Mock<IIrcClient> ircClient;
        private Mock<IGeolocationService> geolocService;
        private Mock<IResponder> responderMock;
        private Mock<IJoinMessageConfigurationService> configService;
        
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
            this.ircClient = new Mock<IIrcClient>();
            this.geolocService = new Mock<IGeolocationService>();
            this.responderMock = new Mock<IResponder>();

            this.blockMonitoringService = new Mock<IBlockMonitoringService>();

            this.configService = new Mock<IJoinMessageConfigurationService>();
            
            this.joinMessageService = new JoinMessageService(
                this.Logger.Object,
                this.responderMock.Object,
                new ModuleConfiguration{JoinMessageRateLimits = new RateLimitConfiguration{RateLimitMax = 1, RateLimitDuration = 10}},
                this.geolocService.Object,
                this.ircClient.Object,
                this.blockMonitoringService.Object,
                new Mock<IChannelManagementService>().Object,
                this.configService.Object,
                new Mock<ICrossChannelService>().Object
                );

            this.configService.Setup(x => x.GetUsers("ab"))
                .Returns(new List<WelcomeUser> { this.welcomeUser });
            this.configService.Setup(x => x.GetUsers("ef"))
                .Returns(new List<WelcomeUser> { this.welcomeUser });
            this.configService.Setup(x => x.GetExceptions("ab"))
                .Returns(new List<WelcomeUser> { this.ignoreUser });
            this.configService.Setup(x => x.GetExceptions("ef"))
                .Returns(new List<WelcomeUser> { this.ignoreUser });
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldNotWelcomeUnknownUser()
        {
            // arrange
            var networkUser = new Mock<IUser>();

            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "cd/test";

            this.joinMessageService.ClearRateLimitCache();

            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);

            // assert
            this.ircClient.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldNotWelcomeIgnoredUser()
        {
            // arrange
            var networkUser = new Mock<IUser>();

            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ign";
            networkUser.Object.Hostname = "ab/test";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);
            
            // assert
            this.ircClient.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldWelcomeUser()
        {
            // arrange
            var networkUser = new Mock<IUser>();
            
            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "ab/test";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            
            this.configService.Setup(x => x.GetOverride(It.IsAny<string>(), It.IsAny<string>())).Returns(() => null);
            
            this.responderMock.Setup(
                    x => x.Respond(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<object[]>(),
                        null,
                        CommandResponseDestination.Default,
                        CommandResponseType.Message,
                        false,
                        null))
                .Returns(new[] { new CommandResponse { Message = "ab" } });
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);

            // assert
            this.ircClient.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Once());
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldNotWelcomeUserOnUnknownChannel()
        {
            // arrange
            this.configService.Setup(x => x.GetUsers("cd"))
                .Returns(new List<WelcomeUser>());
            this.configService.Setup(x => x.GetExceptions("cd"))
                .Returns(new List<WelcomeUser>());

            var networkUser = new Mock<IUser>();

            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "ab/test";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "cd", this.ircClient.Object);

            // act
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);

            // assert
            this.ircClient.Verify(x => x.SendMessage("cd", It.IsAny<string>()), Times.Never());
            this.ircClient.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// The should rate limit.
        /// </summary>
        [Test]
        public void ShouldRateLimitByHostname()
        {
            var networkUser = new Mock<IUser>();
            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "ab/test";

            var networkUser2 = new Mock<IUser>();
            networkUser2.SetupAllProperties();
            networkUser2.Object.Nickname = "ab";
            networkUser2.Object.Username = "ab2";
            networkUser2.Object.Hostname = "ab/test2";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            var ea2 = new JoinEventArgs(null, networkUser2.Object, "ab", this.ircClient.Object);

            this.configService.Setup(x => x.GetOverride(It.IsAny<string>(), It.IsAny<string>())).Returns(() => null);
            
            this.responderMock.Setup(
                    x => x.Respond(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<object[]>(),
                        null,
                        CommandResponseDestination.Default,
                        CommandResponseType.Message,
                        false,
                        null))
                .Returns(new[] { new CommandResponse { Message = "ab" } });
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea2);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);

            // assert
            this.ircClient.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Exactly(2));
        }

        /// <summary>
        /// The should rate limit.
        /// </summary>
        [Test]
        public void ShouldRateLimit()
        {
            var networkUser = new Mock<IUser>();
            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "ab/test";

            this.responderMock.Setup(
                    x => x.Respond(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<object[]>(),
                        null,
                        CommandResponseDestination.Default,
                        CommandResponseType.Message,
                        false,
                        null))
                .Returns(new[] { new CommandResponse { Message = "ab" } });

            this.configService.Setup(x => x.GetOverride(It.IsAny<string>(), It.IsAny<string>())).Returns(() => null);
            
            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);

            // act
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);

            // assert
            this.ircClient.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Once());
        }

        /// <summary>
        /// The should rate limit.
        /// </summary>
        [Test]
        public void ShouldRateLimitByChannel()
        {
            var networkUser = new Mock<IUser>();
            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "ab/test";

            this.joinMessageService.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            var ea2 = new JoinEventArgs(null, networkUser.Object, "ef", this.ircClient.Object);

            this.configService.Setup(x => x.GetOverride(It.IsAny<string>(), It.IsAny<string>())).Returns(() => null);

            this.responderMock.Setup(
                    x => x.Respond(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<object[]>(),
                        null,
                        CommandResponseDestination.Default,
                        CommandResponseType.Message,
                        false,
                        null))
                .Returns(new[] { new CommandResponse { Message = "ab" } });
            
            // act
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea2);
            this.joinMessageService.OnJoinEvent(this.ircClient.Object, ea2);

            // assert
            this.ircClient.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Once());
            this.ircClient.Verify(x => x.SendMessage("ef", It.IsAny<string>()), Times.Once());
        }
    }
}
