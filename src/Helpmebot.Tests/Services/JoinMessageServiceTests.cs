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
    using Helpmebot.ChannelServices.Services;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using Moq;

    using NHibernate;

    using NUnit.Framework;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// The join message service tests.
    /// </summary>
    [TestFixture]
    public class JoinMessageServiceTests : TestBase
    {
        /// <summary>
        /// The message service.
        /// </summary>
        private readonly Mock<IMessageService> messageService = new Mock<IMessageService>();
        
        /// <summary>
        /// The welcome user.
        /// </summary>
        private WelcomeUser welcomeUser;

        /// <summary>
        /// The ignore user.
        /// </summary>
        private WelcomeUser ignoreUser;

        /// <summary>
        /// The join message service.
        /// </summary>
        private Mock<JoinMessageService> joinMessageService;
        
        private Mock<IBlockMonitoringService> blockMonitoringService;

        /// <summary>
        /// The IRC network.
        /// </summary>
        private Mock<IIrcClient> ircClient;

        /// <summary>
        /// The session.
        /// </summary>
        private Mock<ISession> session;

        private Mock<IGeolocationService> geolocService;

        /// <summary>
        /// The setup.
        /// </summary>
        public override void LocalSetup()
        {
            this.welcomeUser = new WelcomeUser { Host = "ab/.*", User = ".*", Nick = ".*" };
            this.ignoreUser = new WelcomeUser { Host = ".*", User = "ign", Nick = ".*", Exception = true };

            this.session = new Mock<ISession>();
        }

        /// <summary>
        /// The test setup.
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            this.ircClient = new Mock<IIrcClient>();
            this.geolocService = new Mock<IGeolocationService>();

            this.blockMonitoringService = new Mock<IBlockMonitoringService>();
            
            this.joinMessageService = new Mock<JoinMessageService>(
                this.Logger.Object,
                this.messageService.Object,
                this.session.Object,
                new JoinMessageServiceConfiguration(1, 10),
                this.geolocService.Object,
                this.ircClient.Object,
                this.blockMonitoringService.Object);

            this.joinMessageService.Setup(x => x.GetWelcomeUsers("ab"))
                .Returns(new List<WelcomeUser> { this.welcomeUser });
            this.joinMessageService.Setup(x => x.GetWelcomeUsers("ef"))
                .Returns(new List<WelcomeUser> { this.welcomeUser });
            this.joinMessageService.Setup(x => x.GetExceptions("ab"))
                .Returns(new List<WelcomeUser> { this.ignoreUser });
            this.joinMessageService.Setup(x => x.GetExceptions("ef"))
                .Returns(new List<WelcomeUser> { this.ignoreUser });

            this.joinMessageService.CallBase = true;
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

            this.joinMessageService.Object.ClearRateLimitCache();

            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            
            // act
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);

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

            this.joinMessageService.Object.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            
            // act
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);
            
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

            this.joinMessageService.Object.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            
            this.joinMessageService.Setup(x => x.GetOverride(It.IsAny<string>())).Returns(() => null);
            
            // act
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);

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
            this.joinMessageService.Setup(x => x.GetWelcomeUsers("cd"))
                .Returns(new List<WelcomeUser>());
            this.joinMessageService.Setup(x => x.GetExceptions("cd"))
                .Returns(new List<WelcomeUser>());

            var networkUser = new Mock<IUser>();

            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "ab/test";

            this.joinMessageService.Object.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "cd", this.ircClient.Object);

            // act
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);

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

            this.joinMessageService.Object.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            var ea2 = new JoinEventArgs(null, networkUser2.Object, "ab", this.ircClient.Object);

            this.joinMessageService.Setup(x => x.GetOverride(It.IsAny<string>())).Returns(() => null);
            
            // act
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea2);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);

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

            this.joinMessageService.Setup(x => x.GetOverride(It.IsAny<string>())).Returns(() => null);
            
            this.joinMessageService.Object.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);

            // act
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);

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

            this.joinMessageService.Object.ClearRateLimitCache();
            var ea = new JoinEventArgs(null, networkUser.Object, "ab", this.ircClient.Object);
            var ea2 = new JoinEventArgs(null, networkUser.Object, "ef", this.ircClient.Object);

            this.joinMessageService.Setup(x => x.GetOverride(It.IsAny<string>())).Returns(() => null);
            
            // act
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea2);
            this.joinMessageService.Object.OnJoinEvent(this.ircClient.Object, ea2);

            // assert
            this.ircClient.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Once());
            this.ircClient.Verify(x => x.SendMessage("ef", It.IsAny<string>()), Times.Once());
        }
    }
}
