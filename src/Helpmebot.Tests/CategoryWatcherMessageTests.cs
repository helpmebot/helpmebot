namespace Helpmebot.Tests;

using System.Collections;
using CategoryWatcher.Services;
using CategoryWatcher.Services.Interfaces;
using CoreServices.Services.Interfaces;
using CoreServices.Services.Messages;
using CoreServices.Services.Messages.Interfaces;
using Helpmebot.Model;
using Moq;
using NUnit.Framework;

public class CategoryWatcherMessageTests : TestBase
{
    private Mock<CategoryWatcher> catWatcher;
    private Mock<IWatcherConfigurationService> watcherConfig;
    private Mock<IResponder> responder;
    private Mock<ILinkerService> linker;
    private Mock<IUrlShorteningService> urlShorteningService;
    private Mock<IResponseManager> responseManager;

    [SetUp]
    public void Setup()
    {
        this.catWatcher = new Mock<CategoryWatcher>();
        this.catWatcher.Setup(x => x.Id).Returns(1);
        this.catWatcher.Setup(x => x.Category).Returns("potato");
        this.catWatcher.Setup(x => x.Keyword).Returns("potato");
        this.catWatcher.Setup(x => x.BaseWikiId).Returns("potato");

        this.watcherConfig = new Mock<IWatcherConfigurationService>();
        this.responder = new Mock<IResponder>();
        this.linker = new Mock<ILinkerService>();
        this.urlShorteningService = new Mock<IUrlShorteningService>();
        this.responseManager = new Mock<IResponseManager>();
    }

    [Test]
    public void ShouldGetItemMessagePart()
    {
        // arrange
        this.watcherConfig.Setup(x => x.GetWatchers()).Returns(new List<CategoryWatcher>());
        
        this.responder.Setup(
                x => x.GetMessagePart(
                    "catwatcher.item.potato.part",
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Context>()))
            .Returns("hallo!");        
        this.responder.Setup(
                x => x.GetMessagePart(
                    "catwatcher.item.default.part",
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Context>()))
            .Returns("nope!");
        
        var service = new CategoryWatcherHelperService(
            null,
            null,
            this.Logger.Object,
            null,
            null,
            this.responder.Object, 
            this.watcherConfig.Object,
            null,
            null,
            this.responseManager.Object
        );

        // act
        var result = service.GetMessagePart("potato", "part", "", "");
        
        // assert
        Assert.That(result, Is.EqualTo("hallo!"));
    }

    [Test]
    [TestCaseSource(typeof(MessageBuilderDataSource))]
    public void ShouldConstructDefaultMessage(List<CategoryWatcherItem> items, bool newItems, bool empty, bool showLinks, bool showWaitTime, int delay, string expected)
    {
        // arrange
        this.responder.Setup(x => x.GetMessagePart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<Context>()))
            .Returns(
                (string key, string ctx, object[] args, Context ct) =>
                {
                    if (ctx != "dest")
                    {
                        throw new ArgumentOutOfRangeException(nameof(ctx));
                    }
                    
                    var s = key switch
                    {
                        "catwatcher.item.test.noitems" => "There are no {0}.",
                        "catwatcher.item.test.hasitems" => "There are {0} {1}: {2}",
                        "catwatcher.item.test.newitems" => "There are {0} new {1}: {2}",
                        "catwatcher.item.test.plural" => "pages",
                        "catwatcher.item.test.singular" => "page",
                        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
                    };

                    return string.Format(s, args ?? Array.Empty<object>());
                });
        this.linker.Setup(x => x.ConvertWikilinkToUrl(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string d, string title) => "https://enwp.org/" + title);
        this.urlShorteningService.Setup(x => x.Shorten(It.IsAny<string>())).Returns((string x) => x);
        
        this.watcherConfig.Setup(x => x.GetWatchers()).Returns(new List<CategoryWatcher>());

        var service = new CategoryWatcherHelperService(
            this.linker.Object,
            this.urlShorteningService.Object,
            this.Logger.Object,
            null,
            null,
            this.responder.Object,
            this.watcherConfig.Object,
            null,
            null,
            this.responseManager.Object
        );

        // act
        var message = service.ConstructResultMessage(items, "test", "dest", newItems, empty, showLinks, showWaitTime, delay);

        // assert
        Assert.That(message, Is.EqualTo(expected));
    }

    private class MessageBuilderDataSource : IEnumerable<object?[]>
    {
        private IEnumerable<object?[]> GetDataset()
        {
            var foo = new CategoryWatcherItem { Title = "foo", InsertTime = DateTime.UtcNow };
            var bar = new CategoryWatcherItem { Title = "bar", InsertTime = DateTime.UtcNow };
            var baz = new CategoryWatcherItem { Title = "baz", InsertTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 1, 1, 0)) };
            var qux = new CategoryWatcherItem { Title = "qux", InsertTime = DateTime.UtcNow.Subtract(new TimeSpan(2, 1, 1, 0)) };
            
            // items, describe new, describe empty, show links, show time, mintime (secs), result
            
            // return nothing
            yield return new object?[]
            {
                new List<CategoryWatcherItem>(), false, false, true, true, 60, null
            };
            
            // return empty list
            yield return new object?[]
            {
                new List<CategoryWatcherItem>(), false, true, true, true, 60, "There are no pages."
            };
            
            // return single item
            yield return new object?[]
            {
                new List<CategoryWatcherItem>{foo}, false, true, false, false, 60, "There are 1 page: [[foo]]"
            };
            
            // return multiple items
            yield return new object?[]
            {
                new List<CategoryWatcherItem>{foo, bar}, false, false, true, true, 60, "There are 2 pages: [[foo]] https://enwp.org/foo , [[bar]] https://enwp.org/bar"
            };
            
            // return multiple new items
            yield return new object?[]
            {
                new List<CategoryWatcherItem>{foo, bar}, true, false, true, true, 60, "There are 2 new pages: [[foo]] https://enwp.org/foo , [[bar]] https://enwp.org/bar"
            };
            
            // return old item
            yield return new object?[]
            {
                new List<CategoryWatcherItem>{baz}, false, false, true, true, 60, "There are 1 page: [[baz]] https://enwp.org/baz (waiting 01:01)"
            };
            
            // return v. old item
            yield return new object?[]
            {
                new List<CategoryWatcherItem>{qux}, false, false, true, true, 60, "There are 1 page: [[qux]] https://enwp.org/qux (waiting 2d 01:01)"
            };
        }

        
        public IEnumerator<object?[]> GetEnumerator()
        {
            return this.GetDataset().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetDataset().GetEnumerator();
        }
    }
}