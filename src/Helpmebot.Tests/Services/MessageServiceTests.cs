namespace Helpmebot.Tests.Services
{
    using System;
    using System.Collections;
    using System.Text;

    using Castle.Core.Logging;

    using Helpmebot.Legacy.Database;
    using Helpmebot.Services;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MessageServiceTests
    {
        private Mock<IDAL> databaseAccessLayer;

        private MessageService messageService;

        [TestFixtureSetUp]
        public void CustomSetup()
        {
            string value = "test {0} {1}";
            byte[] data = Encoding.UTF8.GetBytes(value);

            databaseAccessLayer = new Mock<IDAL>();
            databaseAccessLayer.Setup(x => x.executeSelect(It.IsAny<DAL.Select>()))
                .Returns(new ArrayList { new object[] { data } });

            messageService = new MessageService(databaseAccessLayer.Object);
        }

        [Test]
        public void ShouldGetMessage()
        {
            // arrange

            // act
            var result = this.messageService.RetrieveMessage("test", "context", new[] { "arg1", "arg2" });

            // assert
            Assert.That(result, Is.EqualTo("test arg1 arg2"));
        }

        [Test]
        public void ShouldGetMessageOnNullContext()
        {
            // arrange

            // act
            var result = this.messageService.RetrieveMessage("test", null, new[] { "arg1", "arg2" });

            // assert
            Assert.That(result, Is.EqualTo("test arg1 arg2"));
        }

        [Test]
        public void ShouldGetMessageOnNullArgs()
        {
            // arrange

            // act
            var result = this.messageService.RetrieveMessage("test", "context", null);

            // assert
            Assert.That(result, Is.EqualTo("test {0} {1}"));
        }
    }
}
