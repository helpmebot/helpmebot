// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListExtensionsTests.cs" company="Helpmebot Development Team">
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
//   Defines the ListExtensionsTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.Extensions
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Helpmebot.CoreServices.ExtensionMethods;

    /// <summary>
    /// The list extensions tests.
    /// </summary>
    [TestFixture]
    public class ListExtensionsTests
    {
        /// <summary>
        /// The pop from front test.
        /// </summary>
        [Test]
        public void PopFromFrontTest()
        {
            // arrange
            var data = new List<string> { "foo", "bar", "baz" };

            Assert.That(data.Count, Is.EqualTo(3));

            // act
            string popped = data.PopFromFront();

            // assert
            Assert.That(popped, Is.EqualTo("foo"));
            Assert.That(data.Count, Is.EqualTo(2));
        }
    }
}
