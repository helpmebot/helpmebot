namespace Helpmebot.Tests.IRC
{
    using System.Diagnostics;

    using Helpmebot.IRC;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model.Interfaces;

    using Moq;

    using NUnit.Framework;

    using DataReceivedEventArgs = Helpmebot.IRC.Events.DataReceivedEventArgs;

    /// <summary>
    /// The irc client tests.
    /// </summary>
    [TestFixture]
    public class IrcClientTests : TestBase
    {
        /// <summary>
        /// The test join processed correctly.
        /// </summary>
        [Test]
        public void TestJoinProcessedCorrectly()
        {
            var network = new Mock<INetworkClient>();
            this.IrcConfiguration.Setup(x => x.Nickname).Returns("nickname");
            this.IrcConfiguration.Setup(x => x.Username).Returns("username");
            this.IrcConfiguration.Setup(x => x.RealName).Returns("real name");
            var client = new IrcClient(network.Object, this.Logger.Object, this.IrcConfiguration.Object, string.Empty);

            // init IRC
            // Setup capabilities
            network.Raise(
                x => x.DataReceived += null, 
                new DataReceivedEventArgs(":testnet CAP * ACK :account-notify extended-join multi-prefix"));

            // Complete registration
            network.Raise(x => x.DataReceived += null, new DataReceivedEventArgs(":testnet 001 nickname :Welcome"));

            // Join a channel
            network.Raise(
                x => x.DataReceived += null, 
                new DataReceivedEventArgs(":nickname!username@hostname JOIN #channel * :real name"));

            // Grab the actual user out when a JOIN event is raised
            IUser actualUser = null;
            client.JoinReceivedEvent += (sender, args) => actualUser = args.User;

            // get ChanServ to join the channel
            network.Raise(
                x => x.DataReceived += null, 
                new DataReceivedEventArgs(":ChanServ!ChanServ@services. JOIN #channel * :Channel Services"));

            // Double check we got it
            Assert.That(actualUser, Is.Not.Null);
            Assert.That(actualUser.Nickname, Is.EqualTo("ChanServ"));
            Assert.That(actualUser.Username, Is.EqualTo("ChanServ"));
            Assert.That(actualUser.Hostname, Is.EqualTo("services."));
            Assert.That(actualUser.Account, Is.Null);
        }
    }
}