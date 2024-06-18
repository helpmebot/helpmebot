namespace Helpmebot.Tests.Model
{
    using Helpmebot.ChannelServices.Model.ModeMonitoring;
    using NUnit.Framework;

    [TestFixture]
    public class ModeChangesTests : TestBase
    {
        [Test]
        public void ShouldParseRemovalOfParameterisedModes()
        {
            var fromChangeList = ModeChanges.FromChangeList(new []{"+g-icfnt"});

            Assert.That(fromChangeList.Bans, Is.Empty);
            Assert.That(fromChangeList.Quiets, Is.Empty);
            Assert.That(fromChangeList.Unbans, Is.Empty);
            Assert.That(fromChangeList.Unquiets, Is.Empty);
            Assert.That(fromChangeList.Exempts, Is.Empty);
            Assert.That(fromChangeList.Unexempts, Is.Empty);
            Assert.That(fromChangeList.Ops, Is.Empty);
            Assert.That(fromChangeList.Deops, Is.Empty);

            Assert.That(fromChangeList.Moderated, Is.Null);
            Assert.That(fromChangeList.ReducedModeration, Is.Null);
            Assert.That(fromChangeList.RegisteredOnly, Is.Null);
        }
    }
}