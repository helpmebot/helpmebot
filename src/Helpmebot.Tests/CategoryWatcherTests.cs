namespace Helpmebot.Tests;

using CategoryWatcher.Services;
using CategoryWatcher.Services.Interfaces;
using CoreServices.Services.Interfaces;
using Helpmebot.Model;
using Moq;
using NUnit.Framework;
using Stwalkerster.Bot.CommandLib.Services.Interfaces;
using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

public class CategoryWatcherTests : TestBase
{
    private Mock<CategoryWatcher> catWatcher;
    private Mock<IItemPersistenceService> persistenceService;
    private Mock<ICommandParser> commandParser;
    private Mock<IWatcherConfigurationService> watcherConfig;
    private Mock<IMediaWikiApi> api;
    private Mock<IMediaWikiApiHelper> apiHelper;

    [SetUp]
    public void Setup()
    {
        this.catWatcher = new Mock<CategoryWatcher>();
        this.catWatcher.Setup(x => x.Id).Returns(1);
        this.catWatcher.Setup(x => x.Category).Returns("potato");
        this.catWatcher.Setup(x => x.Keyword).Returns("potato");
        this.catWatcher.Setup(x => x.BaseWikiId).Returns("potato");

        this.commandParser = new Mock<ICommandParser>();
        this.persistenceService = new Mock<IItemPersistenceService>();
        this.watcherConfig = new Mock<IWatcherConfigurationService>();
        this.api = new Mock<IMediaWikiApi>();
        this.apiHelper = new Mock<IMediaWikiApiHelper>();
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
        this.persistenceService.Setup(x => x.GetItems(It.IsAny<string>()))
            .Returns(
                new List<CategoryWatcherItem>
                {
                    new() { Title = "foo" },
                    new() { Title = "bar" },
                    new() { Title = "quux" }
                })
            .Verifiable();
        this.persistenceService.Setup(x => x.AddNewItems(It.IsAny<string>(), It.IsAny<List<string>>()))
            .Returns(new List<CategoryWatcherItem> { new() { Title = "baz" } });

        this.watcherConfig.Setup(x => x.GetWatchers()).Returns(new List<CategoryWatcher>());

        var service = new CategoryWatcherHelperService(
            null,
            null,
            this.Logger.Object,
            null,
            null,
            null,
            this.watcherConfig.Object,
            this.persistenceService.Object
        );

        //act
        service.SyncItemsToDatabase(new List<string> { "foo", "bar", "baz" }, "foo");

        //assert
        this.persistenceService.Verify(
            x => x.AddNewItems("foo", It.Is<List<string>>(list => list.Count == 1 && list[0] == "baz")),
            Times.Once());
        this.persistenceService.Verify(
            x => x.RemoveDeletedItems("foo", It.Is<List<string>>(list => list.Count == 1 && list[0] == "quux")),
            Times.Once);
    }

    [Test]
    public void ShouldFetchItemsByWatcherName()
    {
        // arrange
        this.persistenceService.Setup(x => x.GetIgnoredPages()).Returns(new List<string>());
        
        this.watcherConfig.Setup(x => x.GetWatchers())
            .Returns(new List<CategoryWatcher> { this.catWatcher.Object });

        this.api.Setup(x => x.GetPagesInCategory(It.IsAny<string>()))
            .Returns(new List<string> { "foo", "bar" });

        this.apiHelper.Setup(x => x.GetApi(It.IsAny<string>(), It.IsAny<bool>())).Returns(this.api.Object);

        var service = new CategoryWatcherHelperService(
            null,
            null,
            this.Logger.Object,
            this.commandParser.Object,
            this.apiHelper.Object,
            null,
            this.watcherConfig.Object,
            this.persistenceService.Object
        );

        // act
        var items = service.FetchCategoryItems("potato");

        // assert
        this.api.Verify(x => x.GetPagesInCategory("potato"), Times.Once);
        this.apiHelper.Verify(x => x.GetApi("potato", true), Times.Once);
        this.apiHelper.Verify(x => x.Release(this.api.Object), Times.Once);

        Assert.That(items, Has.Count.EqualTo(2));
    }    
    [Test]
    public void ShouldFetchNonIgnoredItems()
    {
        // arrange
        this.persistenceService.Setup(x => x.GetIgnoredPages()).Returns(new List<string>{"foo"});
        
        this.watcherConfig.Setup(x => x.GetWatchers())
            .Returns(new List<CategoryWatcher> { this.catWatcher.Object });

        this.api.Setup(x => x.GetPagesInCategory(It.IsAny<string>()))
            .Returns(new List<string> { "foo", "bar" });

        this.apiHelper.Setup(x => x.GetApi(It.IsAny<string>(), It.IsAny<bool>())).Returns(this.api.Object);

        var service = new CategoryWatcherHelperService(
            null,
            null,
            this.Logger.Object,
            this.commandParser.Object,
            this.apiHelper.Object,
            null,
            this.watcherConfig.Object,
            this.persistenceService.Object
        );

        // act
        var items = service.FetchCategoryItems("potato");

        // assert
        this.api.Verify(x => x.GetPagesInCategory("potato"), Times.Once);
        this.apiHelper.Verify(x => x.GetApi("potato", true), Times.Once);
        this.apiHelper.Verify(x => x.Release(this.api.Object), Times.Once);

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("bar"));
    }
}