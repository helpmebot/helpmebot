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

        /// <summary>
        /// HMB-169
        /// </summary>
        [Test]
        public void TestUserFleshedOnJoin()
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

            var data = new[]
                           {
                               ":nickname!username@hostname JOIN #wikipedia-en-helpers * :real name",
                               ":testnet 332 nickname #wikipedia-en-helpers :Channel topic here",
                               ":testnet 333 nickname #wikipedia-en-helpers Matthew_!~Matthewrb@wikimedia/matthewrbowker 1453362294",
                               ":testnet 353 nickname = #wikipedia-en-helpers :nickname FastLizard4",
                               ":testnet 366 nickname #wikipedia-en-helpers :End of /NAMES list."
                           };

            // Join a channel
            foreach (var s in data)
            {
                network.Raise(x => x.DataReceived += null, new DataReceivedEventArgs(s));
            }

            Assert.That(client.UserCache.ContainsKey("FastLizard4"));

            // OK, Flizzy should still be a skeleton.
            Assert.That(client.UserCache["FastLizard4"].Skeleton);

            // ... and stwalkerster shouldn't exist.
            Assert.That(client.UserCache.ContainsKey("stwalkerster"), Is.False);

            // stwalkerster joins the channel
            var join = ":stwalkerster!~stwalkers@wikimedia/stwalkerster JOIN #wikipedia-en-helpers stwalkerster :realname";
            network.Raise(x => x.DataReceived += null, new DataReceivedEventArgs(join));

            // ... and stwalkerster should now exist as a real user
            Assert.That(client.UserCache.ContainsKey("stwalkerster"));
            Assert.That(client.UserCache["stwalkerster"].Skeleton, Is.False);
            Assert.That(client.UserCache["stwalkerster"].Username, Is.EqualTo("~stwalkers"));
            Assert.That(client.UserCache["stwalkerster"].Hostname, Is.EqualTo("wikimedia/stwalkerster"));
            Assert.That(client.UserCache["stwalkerster"].Account, Is.EqualTo("stwalkerster"));

            // Flizzy does a /nick
            var nick = ":FastLizard4!fastlizard@wikipedia/pdpc.active.FastLizard4 NICK :werelizard";
            network.Raise(x => x.DataReceived += null, new DataReceivedEventArgs(nick));

            // ... and werelizard should now exist as a real user, but not Flizzy
            Assert.That(client.UserCache.ContainsKey("FastLizard4"), Is.False);
            Assert.That(client.UserCache.ContainsKey("werelizard"), Is.True);
            Assert.That(client.UserCache["werelizard"].Skeleton, Is.False);
            Assert.That(client.UserCache["werelizard"].Username, Is.EqualTo("fastlizard"));
            Assert.That(client.UserCache["werelizard"].Hostname, Is.EqualTo("wikipedia/pdpc.active.FastLizard4"));
            Assert.That(client.UserCache["werelizard"].Nickname, Is.EqualTo("werelizard"));
        }
    }
}