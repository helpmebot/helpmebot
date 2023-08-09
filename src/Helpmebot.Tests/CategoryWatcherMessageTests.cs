namespace Helpmebot.Tests;

using System.Collections;
using CategoryWatcher.Services;
using CategoryWatcher.Services.Interfaces;
using CoreServices.Services.Interfaces;
using CoreServices.Services.Messages;
using CoreServices.Services.Messages.Interfaces;
using Helpmebot.Model;
using NSubstitute;
using NUnit.Framework;

public class CategoryWatcherMessageTests : TestBase
{
    private CategoryWatcher catWatcher;
    private IWatcherConfigurationService watcherConfig;
    private IResponder responder;
    private ILinkerService linker;
    private IUrlShorteningService urlShorteningService;
    private IResponseManager responseManager;

    [SetUp]
    public void Setup()
    {
        this.catWatcher = Substitute.For<CategoryWatcher>();
        this.catWatcher.Id.Returns(1);
        this.catWatcher.Category.Returns("potato");
        this.catWatcher.Keyword.Returns("potato");
        this.catWatcher.BaseWikiId.Returns("potato");

        this.watcherConfig = Substitute.For<IWatcherConfigurationService>();
        this.responder = Substitute.For<IResponder>();
        this.linker = Substitute.For<ILinkerService>();
        this.urlShorteningService = Substitute.For<IUrlShorteningService>();
        this.responseManager = Substitute.For<IResponseManager>();
    }

    [Test]
    public void ShouldGetItemMessagePart()
    {
        // arrange
        this.watcherConfig.GetWatchers().Returns(new List<CategoryWatcher>());
        
        this.responder.GetMessagePart(
                    "catwatcher.item.potato.part",
                    Arg.Any<string>(),
                    Arg.Any<object[]>(),
                    Arg.Any<Context>())
            .Returns("hallo!");        
        this.responder.GetMessagePart(
                    "catwatcher.item.default.part",
                    Arg.Any<string>(),
                    Arg.Any<object[]>(),
                    Arg.Any<Context>())
            .Returns("nope!");
        
        var service = new CategoryWatcherHelperService(
            null,
            null,
            this.Logger,
            null,
            null,
            this.responder, 
            this.watcherConfig,
            null,
            null,
            this.responseManager
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
        this.responder.GetMessagePart(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object[]>(), Arg.Any<Context>())
            .Returns(
                ci =>
                {
                    var key = ci.ArgAt<string>(0);
                    var ctx = ci.ArgAt<string>(1);

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

                    return string.Format(s, ci.ArgAt<object[]>(2) ?? Array.Empty<object>());
                });
        
        this.linker.ConvertWikilinkToUrl(Arg.Any<string>(), Arg.Any<string>())
            .Returns(ci => "https://enwp.org/" + ci.ArgAt<string>(1));

        this.urlShorteningService.Shorten(Arg.Any<string>()).Returns(ci => ci.Arg<string>());
        
        this.watcherConfig.GetWatchers().Returns(new List<CategoryWatcher>());

        var service = new CategoryWatcherHelperService(
            this.linker,
            this.urlShorteningService,
            this.Logger,
            null,
            null,
            this.responder,
            this.watcherConfig,
            null,
            null,
            this.responseManager
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