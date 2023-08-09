namespace Helpmebot.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpmebot.Commands.Model;
    using Helpmebot.Commands.Services;
    using Helpmebot.Commands.Configuration;
    using NSubstitute;
    using NUnit.Framework;
    using Stwalkerster.Bot.MediaWikiLib.Model;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public class DraftStatusServiceTests : TestBase
    {
        private AfcCategoryConfiguration catConfig;
        private IMediaWikiApi mediaWikiApi;

        public override void LocalSetup()
        {
            this.catConfig = new AfcCategoryConfiguration{
                RejectedCategories = new Dictionary<string, string>
                {
                    {"Category:Rejected AfC submissions",null},
                    {"Category:AfC submissions rejected as non-notable",""},
                    {"Category:AfC submissions rejected as unencyclopedic",""},
                },
                DeclinedCategories = new Dictionary<string, string>
                {
                    {"Category:AfC submissions declined as a non-notable organization", ""},
                    {"Category:AfC submissions declined as non-notable", ""},
                    {"Category:AfC submissions declined with a custom reason", ""},
                    {"Category:AfC submissions declined as lacking reliable third-party sources", ""},
                    {"Category:AfC submissions declined as an advertisement", ""},
                },
                InReviewCategories = new Dictionary<string, string> {{"Category:Pending AfC submissions being reviewed now", null}},
                DraftCategories = new Dictionary<string, string> {{"Category:Draft AfC submissions", null}},
                PendingCategories = new Dictionary<string, string> {{"Category:Pending AfC submissions", null}},
                InArticleSpaceCategories = new Dictionary<string, string> {{"Category:Pending AfC submissions in article space", null}},
                SpeedyDeletionCategories = new Dictionary<string, string> {{"Category:Candidates for speedy deletion", null}}
            };
            
            this.mediaWikiApi = Substitute.For<IMediaWikiApi>();
        }

        [Test, TestCaseSource(typeof(DraftStatusServiceTests), nameof(StatusTestCases))]
        public void ShouldReportStatusCorrectly(string[] categories, Tuple<DraftStatusCode, DateTime?> expectedResult)
        {
            // arrange
            var service = new DraftStatusService(this.catConfig, this.Logger);
            var enumerable = AdaptCategoriesForTest(categories);
            this.mediaWikiApi.GetCategoriesOfPage(Arg.Any<string>()).Returns(enumerable);

            // act
            var result = service.GetDraftStatus(this.mediaWikiApi, "");

            // assert
            Assert.That(result.StatusCode, Is.EqualTo(expectedResult.Item1));
        }
        
        [Test, TestCaseSource(typeof(DraftStatusServiceTests), nameof(StatusTestCases))]
        public void ShouldReportSubmissionDateCorrectly(string[] categories, Tuple<DraftStatusCode, DateTime?> expectedResult)
        {
            // arrange
            var service = new DraftStatusService(this.catConfig, this.Logger);
            var enumerable = AdaptCategoriesForTest(categories);
            this.mediaWikiApi.GetCategoriesOfPage(Arg.Any<string>()).Returns(enumerable);

            // act
            var result = service.GetDraftStatus(this.mediaWikiApi, "");

            // assert
            Assert.That(result.SubmissionDate, Is.EqualTo(expectedResult.Item2));
        }

        private static Dictionary<string, PageCategoryProperties> AdaptCategoriesForTest(IEnumerable<string> categories)
        {
            return categories.Select(
                    x =>
                    {
                        var sortKey = "";
                        if (x.Contains("|"))
                        {
                            var strings = x.Split('|');
                            x = strings[0];
                            sortKey = strings[0];
                        }

                        return new KeyValuePair<string, PageCategoryProperties>(
                            "Category:" + x,
                            new PageCategoryProperties(sortKey, false));
                    })
                .ToDictionary(x => x.Key, y => y.Value);
        }

        public static IEnumerable<TestCaseData> StatusTestCases
        {
            get
            {
                yield return new TestCaseData(
                        new[]
                        {
                            "Pending AfC submissions|D",
                            "AfC submissions by date/09 October 2019"
                        },
                        new Tuple<DraftStatusCode, DateTime?>(DraftStatusCode.Pending, new DateTime(2019, 10, 9)))
                    .SetName("Pending with dupe");
                
                yield return new TestCaseData(
                        new[]
                        {
                            "Pending AfC submissions","AfC pending submissions without an age","Undated AfC submissions"
                        },
                        new Tuple<DraftStatusCode, DateTime?>(DraftStatusCode.Pending, null))
                    .SetName("Pending with no date");

                yield return new TestCaseData(
                        new[]
                        {
                            "Pending AfC submissions", "Pending AfC submissions being reviewed now",
                            "AfC submissions by date/09 October 2019"
                        },
                        new Tuple<DraftStatusCode, DateTime?>(DraftStatusCode.InReviewNow, new DateTime(2019, 10, 9)))
                    .SetName("Simple inreview");

                yield return new TestCaseData(
                        new[]
                        {
                            "Draft AfC submissions", "AfC submissions by date/09 October 2019",
                            "Candidates for speedy deletion", "Candidates for speedy deletion as nonsense pages"
                        },
                        new Tuple<DraftStatusCode, DateTime?>(DraftStatusCode.SpeedyDeletion, new DateTime(2019, 10, 9)))
                    .SetName("Draft but speedied");

                yield return new TestCaseData(
                        new[]
                        {
                            "AfC submissions by date/05 October 2019", "Pending AfC submissions",
                            "Pending AfC submissions in article space", "AfC submissions by date/06 October 2019"
                        },
                        new Tuple<DraftStatusCode, DateTime?>(DraftStatusCode.Pending, new DateTime(2019, 10, 6)))
                    .SetName("pending in article space");
                
                yield return new TestCaseData(
                        new[]
                        {
                            "AfC submissions by date/01 September 2017", "AfC submissions by date/05 May 2017",
                            "AfC submissions by date/20 July 2015", "AfC submissions by date/13 May 2015",
                            "AfC submissions by date/18 September 2014", "AfC submissions by date/29 July 2014",
                            "AfC submissions by date/01 July 2014", "AfC submissions by date/24 June 2014",
                            "Pending AfC submissions", "AfC pending submissions by age/Very old",
                            "AfC submissions by date/07 June 2019",
                            "AfC submissions declined as a non-notable organization", "Declined AfC submissions",
                            "AfC submissions declined as non-notable", "AfC submissions declined with a custom reason"
                        },
                        new Tuple<DraftStatusCode, DateTime?>(DraftStatusCode.Pending, new DateTime(2019, 6, 7)))
                    .SetName("pending with previous decline");

                yield return new TestCaseData(
                        new[]
                        {
                            "AfC submissions by date/15 February 2019", "AfC submissions by date/16 December 2018",
                            "AfC submissions by date/11 December 2018", "AfC submissions by date/10 December 2018",
                            "Pending AfC submissions", "AfC pending submissions by age/4 weeks ago",
                            "AfC submissions by date/09 September 2019",
                            "AfC submissions declined as lacking reliable third-party sources",
                            "Declined AfC submissions",
                            "AfC submissions declined with a custom reason", "Rejected AfC submissions"
                        },
                        new Tuple<DraftStatusCode, DateTime?>(DraftStatusCode.Pending, new DateTime(2019, 9, 9)))
                    .SetName("pending with previous decline/rejected");

                yield return new TestCaseData(
                        new[]
                        {
                            "AfC submissions by date/03 April 2019", "AfC submissions by date/11 March 2019",
                            "AfC submissions by date/20 February 2019", "Draft articles",
                            "AfC submissions rejected as non-notable", "AfC submissions rejected as unencyclopedic",
                            "Rejected AfC submissions", "AfC submissions declined as an advertisement",
                            "AfC submissions declined as a non-notable organization", "Declined AfC submissions"
                        },
                        new Tuple<DraftStatusCode, DateTime?>(DraftStatusCode.Rejected, new DateTime(2019, 4, 3)))
                    .SetName("rejected with previous decline");

                yield return new TestCaseData(
                        new[]
                        {
                            "AfC submissions rejected as non-notable", "Rejected AfC submissions in userspace",
                            "Rejected AfC submissions", "AfC submissions by date/16 May 2019"
                        },
                        new Tuple<DraftStatusCode, DateTime?>(DraftStatusCode.Rejected, new DateTime(2019, 5, 16)))
                    .SetName("simple reject");
            }
        }
    }
}