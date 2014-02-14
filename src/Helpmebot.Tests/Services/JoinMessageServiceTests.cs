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

    using Castle.Core.Logging;

    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services;
    using Helpmebot.Services.Interfaces;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// The join message service tests.
    /// </summary>
    [TestFixture]
    public class JoinMessageServiceTests
    {
        /// <summary>
        /// The message service.
        /// </summary>
        private readonly Mock<IMessageService> messageService = new Mock<IMessageService>();

        /// <summary>
        /// The user repo.
        /// </summary>
        private readonly Mock<IWelcomeUserRepository> userRepo = new Mock<IWelcomeUserRepository>();

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly Mock<ILogger> logger = new Mock<ILogger>();
        
        /// <summary>
        /// The setup.
        /// </summary>
        [TestFixtureSetUp]
        public void Setup()
        {
            var welcomeUser = new WelcomeUser { Host = "ab/.*", User = ".*", Nick = ".*" };
            var ignoreUser = new WelcomeUser { Host = ".*", User = "ign", Nick = ".*" };

            this.userRepo.Setup(x => x.GetWelcomeForChannel("ab"))
                .Returns(new List<WelcomeUser> { welcomeUser });

            this.userRepo.Setup(x => x.GetExceptionsForChannel("ab"))
                .Returns(new List<WelcomeUser> { ignoreUser });
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldNotWelcomeUnknownUser()
        {
            // arrange
            var ircNetwork = new Mock<IIrcClient>();
            var joinMessageService = new JoinMessageService(
                ircNetwork.Object,
                this.logger.Object,
                this.userRepo.Object,
                this.messageService.Object);

            var networkUser = new Mock<IUser>();

            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "cd/test";

            // act
            joinMessageService.Welcome(networkUser.Object, "ab");

            // assert
            ircNetwork.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test, Ignore]
        public void ShouldNotWelcomeIgnoredUser()
        {
            // arrange
            var ircNetwork = new Mock<IIrcClient>();
            var joinMessageService = new JoinMessageService(
                ircNetwork.Object,
                this.logger.Object,
                this.userRepo.Object,
                this.messageService.Object);

            var networkUser = new Mock<IUser>();

            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ign";
            networkUser.Object.Hostname = "ab/test";

            // act
            joinMessageService.Welcome(networkUser.Object, "ab");

            // assert
            ircNetwork.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldWelcomeUser()
        {
            // arrange
            var ircNetwork = new Mock<IIrcClient>();
            var joinMessageService = new JoinMessageService(
                ircNetwork.Object,
                this.logger.Object,
                this.userRepo.Object,
                this.messageService.Object);

            var networkUser = new Mock<IUser>();
            
            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "ab/test";

            // act
            joinMessageService.Welcome(networkUser.Object, "ab");

            // assert
            ircNetwork.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Once());
        }

        /// <summary>
        /// The should get message.
        /// </summary>
        [Test]
        public void ShouldNotWelcomeUserOnUnknownChannel()
        {
            // arrange
            var ircNetwork = new Mock<IIrcClient>();
            var joinMessageService = new JoinMessageService(
                ircNetwork.Object,
                this.logger.Object,
                this.userRepo.Object,
                this.messageService.Object);

            var networkUser = new Mock<IUser>();

            networkUser.SetupAllProperties();
            networkUser.Object.Nickname = "ab";
            networkUser.Object.Username = "ab";
            networkUser.Object.Hostname = "ab/test";

            // act
            joinMessageService.Welcome(networkUser.Object, "cd");

            // assert
            ircNetwork.Verify(x => x.SendMessage("cd", It.IsAny<string>()), Times.Never());
            ircNetwork.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Never());
        }
    }
}
