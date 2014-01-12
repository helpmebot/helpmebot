// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcUserTests.cs" company="Helpmebot Development Team">
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
//   The irc user tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.IRC.Model
{
    using Helpmebot.IRC.Model;

    using NUnit.Framework;

    /// <summary>
    /// The IRC user tests.
    /// </summary>
    [TestFixture]
    public class IrcUserTests : TestBase
    {
        /// <summary>
        /// The should create from prefix.
        /// </summary>
        [Test]
        public void ShouldCreateFromPrefix()
        {
            // arrange
            const string Prefix = "Yetanotherx|afk!~Yetanothe@mcbouncer.com";
            var expected = new IrcUser
                               {
                                   Hostname = "mcbouncer.com",
                                   Username = "~Yetanothe",
                                   Nickname = "Yetanotherx|afk"
                               };

            // act
            var actual = IrcUser.FromPrefix(Prefix);

            // assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /// <summary>
        /// The should create from prefix 2.
        /// </summary>
        [Test]
        public void ShouldCreateFromPrefix2()
        {
            // arrange
            const string Prefix = "stwalkerster@foo.com";
            var expected = new IrcUser
            {
                Hostname = "foo.com",
                Nickname = "stwalkerster"
            };

            // act
            var actual = IrcUser.FromPrefix(Prefix);

            // assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /// <summary>
        /// The should create from prefix 3.
        /// </summary>
        [Test]
        public void ShouldCreateFromPrefix3()
        {
            // arrange
            const string Prefix = "stwalkerster";
            var expected = new IrcUser
            {
                Nickname = "stwalkerster"
            };

            // act
            var actual = IrcUser.FromPrefix(Prefix);

            // assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /// <summary>
        /// The should create from prefix.
        /// </summary>
        [Test]
        public void ShouldCreateFromPrefix4()
        {
            // arrange
            const string Prefix = "nick!user@host";
            var expected = new IrcUser
            {
                Hostname = "host",
                Username = "user",
                Nickname = "nick"
            };

            // act
            var actual = IrcUser.FromPrefix(Prefix);

            // assert
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
