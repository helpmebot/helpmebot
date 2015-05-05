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

    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services;
    using Helpmebot.Services.Interfaces;

    using Moq;

    using NHibernate;

    using NUnit.Framework;

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

        /// <summary>
        /// The IRC network.
        /// </summary>
        private Mock<IIrcClient> ircNetwork;

        /// <summary>
        /// The session.
        /// </summary>
        private Mock<ISession> session;

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
            this.ircNetwork = new Mock<IIrcClient>();

            this.joinMessageService = new Mock<JoinMessageService>(
                this.ircNetwork.Object,
                this.Logger.Object,
                this.messageService.Object,
                this.session.Object);

            this.joinMessageService.Setup(x => x.GetWelcomeUsers("ab"))
                .Returns(new List<WelcomeUser> { this.welcomeUser });
            this.joinMessageService.Setup(x => x.GetExceptions("ab"))
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

            // act
            this.joinMessageService.Object.Welcome(networkUser.Object, "ab");

            // assert
            this.ircNetwork.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Never());
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

            // act
            this.joinMessageService.Object.Welcome(networkUser.Object, "ab");

            // assert
            this.ircNetwork.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Never());
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

            // act
            this.joinMessageService.Object.Welcome(networkUser.Object, "ab");

            // assert
            this.ircNetwork.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Once());
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

            // act
            this.joinMessageService.Object.Welcome(networkUser.Object, "cd");

            // assert
            this.ircNetwork.Verify(x => x.SendMessage("cd", It.IsAny<string>()), Times.Never());
            this.ircNetwork.Verify(x => x.SendMessage("ab", It.IsAny<string>()), Times.Never());
        }
    }
}
