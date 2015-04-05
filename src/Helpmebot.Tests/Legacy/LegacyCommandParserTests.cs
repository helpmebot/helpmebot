// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyCommandParserTests.cs" company="Helpmebot Development Team">
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
//   The legacy command parser tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.Legacy
{
    using System.Collections;
    using System.Collections.Generic;

    using Helpmebot.Legacy;

    using NUnit.Framework;

    /// <summary>
    /// The legacy command parser tests.
    /// </summary>
    [TestFixture]
    public class LegacyCommandParserTests : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The test find redirection.
        /// </summary>
        /// <param name="inputdata">
        /// The input data.
        /// </param>
        /// <param name="expecteddata">
        /// The expected data.
        /// </param>
        /// <param name="expectedRedir">
        /// The expected redir.
        /// </param>
        //[TestCaseSource(typeof(RedirectionDataSource))]
        [TestCase("a b c", "a b c", "")]
        [TestCase("a", "a", "")]
        [TestCase(">foo", "", "foo")]
        [TestCase("> foo", "", "foo", Ignore = true)]
        [TestCase("a >b c", "a c", "b")]
        [TestCase("a > b c", "a c", "b", Ignore = true)]
        [TestCase("a >b >c d", "a d", "b c", Ignore = true)]
        [TestCase("a >b", "a", "b")]
        [TestCase(">a b", "b", "a")]
        [TestCase("a > b", "a", "b", Ignore = true)]
        [TestCase("> a b", "b", "a", Ignore = true)]
        [TestCase("a> b", "a> b", "")]
        [TestCase("a >", "a >", "")]
        [TestCase("a b >>>", "a b >>>", "", Ignore = true)]
        public void TestFindRedirection(string inputdata, string expecteddata, string expectedRedir)
        {
            // arrange
            string[] input = inputdata.Split();

            // act
            string redir = LegacyCommandParser.FindRedirection(ref input);

            // assert
            Assert.That(redir, Is.EqualTo(expectedRedir));
        }

        /// <summary>
        /// The test redirection result.
        /// </summary>
        /// <param name="inputdata">
        /// The input data.
        /// </param>
        /// <param name="expecteddata">
        /// The expected data.
        /// </param>
        /// <param name="expectedRedir">
        /// The expected redir.
        /// </param>
        //[TestCaseSource(typeof(RedirectionDataSource))]
        [TestCase("a b c", "a b c", "")]
        [TestCase("a", "a", "")]
        [TestCase(">foo", "", "foo")]
        [TestCase("> foo", "", "foo", Ignore = true)]
        [TestCase("a >b c", "a c", "b")]
        [TestCase("a > b c", "a c", "b", Ignore = true)]
        [TestCase("a >b >c d", "a d", "b c", Ignore = true)]
        [TestCase("a >b", "a", "b")]
        [TestCase(">a b", "b", "a")]
        [TestCase("a > b", "a", "b", Ignore = true)]
        [TestCase("> a b", "b", "a", Ignore = true)]
        [TestCase("a> b", "a> b", "")]
        [TestCase("a >", "a >", "", Ignore = true)]
        [TestCase("a b >>>", "a b >>>", "", Ignore = true)]
        public void TestPostRedirectionMessage(string inputdata, string expecteddata, string expectedRedir)
        {
            // arrange
            string[] input = inputdata.Split();

            string[] expected = expecteddata.Split();
            if (expecteddata == string.Empty)
            {
                expected = new string[0];
            }

            // act
            string redir = LegacyCommandParser.FindRedirection(ref input);

            // assert
            Assert.That(input, Is.EqualTo(expected));
        }

        #endregion

        /// <summary>
        /// The redirection data source.
        /// </summary>
        private class RedirectionDataSource : IEnumerable<string[]>
        {
            #region Fields

            /// <summary>
            /// The data.
            /// </summary>
            private readonly List<string[]> data = new List<string[]>
                                                       {
                                                           new[] { "a b c", "a b c", string.Empty }, 
                                                           new[] { "a", "a",  string.Empty }, 
                                                           new[] { ">foo", string.Empty, "foo" }, 
                                                           new[] { "> foo", string.Empty, "foo" }, 
                                                           new[] { "a >b c", "a c", "b" }, 
                                                           new[] { "a > b c", "a c", "b" }, 
                                                           new[] { "a >b >c d", "a d", "b c" }, 
                                                           new[] { "a >b", "a", "b" }, 
                                                           new[] { ">a b", "b", "a" }, 
                                                           new[] { "a > b", "a", "b" }, 
                                                           new[] { "> a b", "b", "a" }, 
                                                           new[] { "a> b", "a> b",  string.Empty }, 
                                                           new[] { "a >", "a >",  string.Empty }, 
                                                           new[] { "a b >>>", "a b >>>",  string.Empty }, 
                                                       };

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<string[]> GetEnumerator()
            {
                return this.data.GetEnumerator();
            }

            #endregion

            #region Explicit Interface Methods

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }
    }
}