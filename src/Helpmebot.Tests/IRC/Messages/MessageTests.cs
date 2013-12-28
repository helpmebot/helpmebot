// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageTests.cs" company="Helpmebot Development Team">
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
//   Defines the MessageTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.IRC.Messages
{
    using System.Collections.Generic;

    using Helpmebot.IRC.Messages;

    using NUnit.Framework;

    /// <summary>
    /// The message tests.
    /// </summary>
    [TestFixture]
    public class MessageTests
    {
        /// <summary>
        /// The should parse correctly.
        /// </summary>
        [Test]
        public void ShouldParseCorrectly()
        { 
            // arrange
            string message = ":server 001 :Welcome!";
            var expected = new Message
                               {
                                   Prefix = "server",
                                   Command = "001",
                                   Parameters = new List<string> { "Welcome!" }
                               };

            this.DoParseTest(message, expected);
        }

        [Test]
        public void ShouldParseCorrectly2()
        {
            // arrange
            string message = ":bla MODE foo bar :do something!";
            var expected = new Message
            {
                Prefix = "bla",
                Command = "MODE",
                Parameters = new List<string> { "foo", "bar", "do something!" }
            };

            this.DoParseTest(message, expected);
        }

        [Test]
        public void ShouldParseCorrectly3()
        {
            // arrange
            string message = "PRSDS";
            var expected = new Message
            {
                Command = "PRSDS",
            };

            this.DoParseTest(message, expected);
        }

        /// <summary>
        /// The do parse test.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="expected">
        /// The expected.
        /// </param>
        private void DoParseTest(string message, Message expected)
        {
            // act
            var actual = (Message)Message.Parse(message);

            // assert
            Assert.That(actual.Prefix, Is.EqualTo(expected.Prefix));
            Assert.That(actual.Command, Is.EqualTo(expected.Command));
            Assert.That(actual.Parameters, Is.EqualTo(expected.Parameters));
        }
    }
}
