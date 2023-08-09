namespace Helpmebot.Tests;

using CategoryWatcher.Services;
using CategoryWatcher.Services.Interfaces;
using CoreServices.Services.Interfaces;
using CoreServices.Services.Messages.Interfaces;
using Helpmebot.Model;
using NSubstitute;
using NUnit.Framework;
using Stwalkerster.Bot.CommandLib.Services.Interfaces;
using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

public class CategoryWatcherTests : TestBase
{
    private CategoryWatcher catWatcher;
    private IItemPersistenceService persistenceService;
    private ICommandParser commandParser;
    private IWatcherConfigurationService watcherConfig;
    private IMediaWikiApi api;
    private IMediaWikiApiHelper apiHelper;
    private IResponseManager responseManager;

    [SetUp]
    public void Setup()
    {
        this.catWatcher = Substitute.For<CategoryWatcher>();
        this.catWatcher.Id.Returns(1);
        this.catWatcher.Category.Returns("potato");
        this.catWatcher.Keyword.Returns("potato");
        this.catWatcher.BaseWikiId.Returns("potato");

        this.commandParser = Substitute.For<ICommandParser>();
        this.persistenceService = Substitute.For<IItemPersistenceService>();
        this.watcherConfig = Substitute.For<IWatcherConfigurationService>();
        this.api = Substitute.For<IMediaWikiApi>();
        this.apiHelper = Substitute.For<IMediaWikiApiHelper>();
        this.responseManager = Substitute.For<IResponseManager>();
    }

    [Test]
    public void ShouldCalculateNilDeltaCorrectly()
    {
        //arrange
        var oldList = new List<string> { "foo", "bar", "baz" };
        var newList = new List<string> { "foo", "bar", "baz" };

        //act
        var (added, removed) = CategoryWatcherHelperService.CalculateListDelta(oldList, newList);

        //assert
        Assert.Multiple(
            () =>
            {
                Assert.That(added, Is.Empty);
                Assert.That(removed, Is.Empty);
            });
    }

    [Test]
    public void ShouldCalculateEmptyListsDeltaCorrectly()
    {
        //arrange
        var oldList = new List<string>();
        var newList = new List<string>();

        //act
        var (added, removed) = CategoryWatcherHelperService.CalculateListDelta(oldList, newList);

        //assert
        Assert.Multiple(
            () =>
            {
                Assert.That(added, Is.Empty);
                Assert.That(removed, Is.Empty);
            });
    }

    [Test]
    public void ShouldCalculateAddedListsDeltaCorrectly()
    {
        //arrange
        var oldList = new List<string> { "foo", "baz" };
        var newList = new List<string> { "foo", "bar", "baz" };

        //act
        var (added, removed) = CategoryWatcherHelperService.CalculateListDelta(oldList, newList);

        //assert
        Assert.Multiple(
            () =>
            {
                Assert.That(added, Has.Count.EqualTo(1));
                Assert.That(removed, Is.Empty);
                Assert.That(added[0], Is.EqualTo("bar"));
            });
    }

    [Test]
    public void ShouldCalculateRemovedListsDeltaCorrectly()
    {
        //arrange
        var oldList = new List<string> { "foo", "bar", "baz" };
        var newList = new List<string> { "foo", "baz" };

        //act
        var (added, removed) = CategoryWatcherHelperService.CalculateListDelta(oldList, newList);

        //assert
        Assert.Multiple(
            () =>
            {
                Assert.That(added, Is.Empty);
                Assert.That(removed, Has.Count.EqualTo(1));
                Assert.That(removed[0], Is.EqualTo("bar"));
            });
    }

    [Test]
    public void ShouldCalculateReplacedListsDeltaCorrectly()
    {
        //arrange
        var oldList = new List<string> { "foo", "bar", "baz" };
        var newList = new List<string> { "foo", "qux", "baz" };

        //act
        var (added, removed) = CategoryWatcherHelperService.CalculateListDelta(oldList, newList);

        //assert
        Assert.Multiple(
            () =>
            {
                Assert.That(added, Has.Count.EqualTo(1));
                Assert.That(added[0], Is.EqualTo("qux"));
                Assert.That(removed, Has.Count.EqualTo(1));
                Assert.That(removed[0], Is.EqualTo("bar"));
            });
    }

    [Test]
    public void ShouldUpdateItemPersistence()
    {
        //arrange
        this.persistenceService.GetItems(Arg.Any<string>())
            .Returns(
                new List<CategoryWatcherItem>
                {
                    new() { Title = "foo" },
                    new() { Title = "bar" },
                    new() { Title = "quux" }
                });
        
        this.persistenceService.AddNewItems(Arg.Any<string>(), Arg.Any<List<string>>()).Returns(new List<CategoryWatcherItem> { new() { Title = "baz" } });

        this.watcherConfig.GetWatchers().Returns(new List<CategoryWatcher>());

        var service = new CategoryWatcherHelperService(
            null,
            null,
            this.Logger,
            null,
            null,
            null,
            this.watcherConfig,
            this.persistenceService,
            null,
            this.responseManager
        );

        //act
        service.SyncItemsToDatabase(new List<string> { "foo", "bar", "baz" }, "foo");

        //assert
        this.persistenceService.Received(1)
            .AddNewItems("foo", Arg.Is<List<string>>(list => list.Count == 1 && list[0] == "baz"));
        this.persistenceService.Received(1)
            .RemoveDeletedItems("foo", Arg.Is<List<string>>(list => list.Count == 1 && list[0] == "quux"));
    }

    [Test]
    public void ShouldFetchItemsByWatcherName()
    {
        // arrange
        this.persistenceService.GetIgnoredPages().Returns(new List<string>());
        
        this.watcherConfig.GetWatchers().Returns(new List<CategoryWatcher> { this.catWatcher });

        this.api.GetPagesInCategory(Arg.Any<string>()).Returns(new List<string> { "foo", "bar" });

        this.apiHelper.GetApi(Arg.Any<string>(), Arg.Any<bool>()).Returns(this.api);

        var service = new CategoryWatcherHelperService(
            null,
            null,
            this.Logger,
            this.commandParser,
            this.apiHelper,
            null,
            this.watcherConfig,
            this.persistenceService,
            null,
            this.responseManager
        );

        // act
        var items = service.FetchCategoryItems("potato");

        // assert
        this.api.Received(1).GetPagesInCategory("potato");
        this.apiHelper.Received(1).GetApi("potato", true);
        this.apiHelper.Received(1).Release(this.api);

        Assert.That(items, Has.Count.EqualTo(2));
    }    
    [Test]
    public void ShouldFetchNonIgnoredItems()
    {
        // arrange
        this.persistenceService.GetIgnoredPages().Returns(new List<string>{"foo"});
        
        this.watcherConfig.GetWatchers().Returns(new List<CategoryWatcher> { this.catWatcher });

        this.api.GetPagesInCategory(Arg.Any<string>()).Returns(new List<string> { "foo", "bar" });

        this.apiHelper.GetApi(Arg.Any<string>(), Arg.Any<bool>()).Returns(this.api);

        var service = new CategoryWatcherHelperService(
            null,
            null,
            this.Logger,
            this.commandParser,
            this.apiHelper,
            null,
            this.watcherConfig,
            this.persistenceService,
            null,
            this.responseManager
        );

        // act
        var items = service.FetchCategoryItems("potato");

        // assert
        
        this.api.Received(1).GetPagesInCategory("potato");
        this.apiHelper.Received(1).GetApi("potato", true);
        this.apiHelper.Received(1).Release(this.api);

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("bar"));
    }
}