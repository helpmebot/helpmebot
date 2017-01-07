using Helpmebot.Services;

namespace Helpmebot.Tests.Services
{
    using System.Collections;
    using System.Collections.Generic;

    using NUnit.Framework;

    /// <summary>
    /// The legacy command parser tests.
    /// </summary>
    [TestFixture]
    public class RedirectionParserServiceTests : TestBase
    {
        /// <summary>
        /// The test find redirection.
        /// </summary>
        /// <param name="inputdata">
        /// The input data.
        /// </param>
        /// <param name="expectedData">
        /// The expected data.
        /// </param>
        /// <param name="expectedRedir">
        /// The expected redir.
        /// </param>
        [TestCaseSource(typeof(RedirectionDataSource))]
        public void TestFindRedirection(string inputdata, string expectedData, string expectedRedir)
        {
            // arrange
            string[] input = inputdata.Split();
            var service = new RedirectionParserService();

            // act
            var result = service.Parse(input);

            // assert
            Assert.That(result.Destination, Is.EqualTo(expectedRedir));
            Assert.That(string.Join(" ", result.Message), Is.EqualTo(expectedData));
        }

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
                new[] { "a >b >c d", "a d", "b, c" },
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