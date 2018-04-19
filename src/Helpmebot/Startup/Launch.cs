namespace Helpmebot.Startup
{
    using System;
    using System.Net;
    using System.Threading;
    using Castle.Core.Logging;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Helpmebot.Repositories.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;

    // TODO: replace me with lib interface
    public interface IApplication
    {
        void Stop();
        void Run();
    }
    
    public class Launch : IApplication
    {
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly IChannelRepository channelRepository;
        private readonly ManualResetEvent exitLock;

        public Launch(ILogger logger, IIrcClient client, IChannelRepository channelRepository)
        {
            this.logger = logger;
            this.client = client;
            this.channelRepository = channelRepository;

            this.StartupTime = DateTime.Now;
            this.exitLock = new ManualResetEvent(false);

            this.client.DisconnectedEvent += (sender, args) => this.Stop();
        }

        public DateTime StartupTime { get; private set; }
        
        public void Stop()
        {
            this.exitLock.Set();
        }

        public void Run()
        {
            foreach (var channel in this.channelRepository.GetEnabled())
            {
                this.client.JoinChannel(channel.Name);
            }
            
            this.logger.Info("Awaiting exit signal.");
            this.exitLock.WaitOne();
            this.logger.Error("Exit signal received!");
        }
        

        internal static void ConfigureCertificateValidation(IWindsorContainer container)
        {
            var disableCertificateValidation = container.Resolve<BotConfiguration>().DisableCertificateValidation;
            
            if (disableCertificateValidation)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            }
        }
    }
}