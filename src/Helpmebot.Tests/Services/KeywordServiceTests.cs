// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeywordServiceTests.cs" company="Helpmebot Development Team">
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
//   Defines the KeywordServiceTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.Services
{
    using System.Collections.Generic;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// The keyword service tests.
    /// </summary>
    [TestFixture]
    public class KeywordServiceTests : TestBase
    {
        /// <summary>
        /// The keyword repository.
        /// </summary>
        private Mock<IKeywordRepository> keywordRepository;

        /// <summary>
        /// The keyword.
        /// </summary>
        private Keyword keyword;

        /// <summary>
        /// The setup.
        /// </summary>
        public override void LocalSetup()
        {
            this.keywordRepository = new Mock<IKeywordRepository>();
            this.keyword = new Keyword { Action = false, Name = "ab", Response = "ab" };
        }

        /// <summary>
        /// The should return keyword.
        /// </summary>
        [Test]
        public void ShouldReturnKeyword()
        {
            // arrange
            var keywordService = new KeywordService(this.keywordRepository.Object, this.Logger.Object);
            this.keywordRepository.Setup(x => x.GetByName("ab"))
                .Returns(
                    new List<Keyword>
                        {
                            this.keyword,
                            new Keyword { Action = false, Name = "ab", Response = "cd" }
                        });

            // act
            var result = keywordService.Get("ab");

            // assert
            Assert.That(result, Is.EqualTo(this.keyword));
        }
    }
}
