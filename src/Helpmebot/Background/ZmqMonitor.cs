using System.Threading;
using Castle.Core;
using Castle.Core.Logging;
using Helpmebot.IRC.Interfaces;
using ZeroMQ;

namespace Helpmebot.Background
{
    public class ZmqMonitor : IStartable
    {
        private readonly IIrcClient client;
        private readonly ILogger logger;
        private bool active;

        public ZmqMonitor(IIrcClient client, ILogger logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public void Start()
        {
            this.active = true;

            this.logger.Info("Starting inbound message socket");
    
            var t = new Thread(this.ThreadWork);
            t.Start();
        }

        private void ThreadWork()
        {
            var sock = new ZSocket(new ZContext(), ZSocketType.SUB);
            sock.Connect("spearow.lon.stwalkerster.net:3357");
            sock.SubscribeAll();

            while (this.active)
            {
                using (var message = sock.ReceiveMessage())
                {
                    var topic = message[0].ReadString();
                    var journal = message[1].ReadString();
                    var additional = message[2].ReadString();
                    
                    this.client.SendMessage("##stwalkerster", string.Format("{0} - {1} {2}", topic, journal, additional));
                }
            }

            sock.Close();
        }

        public void Stop()
        {
            this.logger.Info("Stopping service");
            this.active = false;
        }
    }
}