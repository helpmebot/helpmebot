// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageServiceTests.cs" company="Helpmebot Development Team">
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
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Tests.Services
{
    using System;
    using System.Text;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    ///     The message service tests.
    /// </summary>
    [TestFixture]
    public class MessageServiceTests
    {
        #region Fields

        /// <summary>
        ///     The message service.
        /// </summary>
        private MessageService messageService;

        /// <summary>
        /// The response repository mock.
        /// </summary>
        private Mock<IResponseRepository> responseRepositoryMock;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The custom setup.
        /// </summary>
        [TestFixtureSetUp]
        public void CustomSetup()
        {
            const string Value = "test {0} {1}";

            this.responseRepositoryMock = new Mock<IResponseRepository>();
            this.responseRepositoryMock.Setup(x => x.GetByName(It.IsAny<string>()))
                .Returns(new Response { Text = Encoding.UTF8.GetBytes(Value) });

            this.messageService = new MessageService(this.responseRepositoryMock.Object);
        }

        /// <summary>
        ///     Should get a message when a context and parameter list is passed in
        /// </summary>
        [Test]
        public void ShouldGetMessage()
        {
            // arrange

            // act
            string result = this.messageService.RetrieveMessage("test", "context", new[] { "arg1", "arg2" });

            // assert
            Assert.That(result, Is.EqualTo("test arg1 arg2"));
        }

        /// <summary>
        ///     Should get a message when a context is passed in
        /// </summary>
        [Test]
        public void ShouldGetMessageOnNullArgs()
        {
            // arrange

            // act
            string result = this.messageService.RetrieveMessage("test", "context", null);

            // assert
            Assert.That(result, Is.EqualTo("test {0} {1}"));
        }

        /// <summary>
        ///     Should get a message when a parameter list is passed in
        /// </summary>
        [Test]
        public void ShouldGetMessageOnNullContext()
        {
            // arrange

            // act
            string result = this.messageService.RetrieveMessage("test", null, new[] { "arg1", "arg2" });

            // assert
            Assert.That(result, Is.EqualTo("test arg1 arg2"));
        }

        /// <summary>
        ///     Should get a message when a context is passed in
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldGetMessageOnNullMessage()
        {
            // arrange

            // act
            this.messageService.RetrieveMessage(null, null, null);

            // assert
        }

        #endregion
    }
}