namespace Helpmebot.Tests.Model
{
    using Helpmebot.Model.ModeMonitoring;
    using NUnit.Framework;

    [TestFixture]
    public class ModeChangesTests : TestBase
    {
        [Test]
        public void ShouldParseRemovalOfParameterisedModes()
        {
            var fromChangeList = ModeChanges.FromChangeList(new []{"+g-icfnt"});

            Assert.IsEmpty(fromChangeList.Bans);
            Assert.IsEmpty(fromChangeList.Quiets);
            Assert.IsEmpty(fromChangeList.Unbans);
            Assert.IsEmpty(fromChangeList.Unquiets);
            Assert.IsEmpty(fromChangeList.Exempts);
            Assert.IsEmpty(fromChangeList.Unexempts);
            Assert.IsEmpty(fromChangeList.Ops);
            Assert.IsEmpty(fromChangeList.Deops);

            Assert.IsNull(fromChangeList.Moderated);
            Assert.IsNull(fromChangeList.ReducedModeration);
            Assert.IsNull(fromChangeList.RegisteredOnly);
        }
    }
}