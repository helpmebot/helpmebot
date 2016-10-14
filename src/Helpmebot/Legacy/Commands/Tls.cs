namespace helpmebot6.Commands
{
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    using Castle.Core.Logging;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;

    using Microsoft.Practices.ServiceLocation;

    class Tls : ProtectedCommand
    {
        private static readonly ILogger StaticLogger;

        static Tls()
        {
            StaticLogger = ServiceLocator.Current.GetInstance<ILogger>().CreateChildLogger("TLS");
        }
        
        /// <summary>
        /// Initialises a new instance of the <see cref="GenericCommand"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="commandServiceHelper">
        /// The command Service Helper.
        /// </param>
        public Tls(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        ///     The execute command.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var length = ServicePointManager.ServerCertificateValidationCallback.GetInvocationList().Length;

            if (length == 0)
            {
                StaticLogger.Error("Disabling TLS safety checks!"); // trigger wide-ranging alerts
                ServicePointManager.ServerCertificateValidationCallback += BypassTlsSafetyChecksCallback;
                return new CommandResponseHandler("TLS safety checks disabled!");
            }
            else
            {
                StaticLogger.Error("Enabling TLS safety checks!"); // trigger wide-ranging alerts
                ServicePointManager.ServerCertificateValidationCallback -= BypassTlsSafetyChecksCallback;
                return new CommandResponseHandler("TLS safety checks disabled!");
            }
        }

        private static bool BypassTlsSafetyChecksCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            StaticLogger.Warn("Bypassing SSL checks!");
            return true;
        }

        /// <summary>
        ///     The not confirmed.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected override CommandResponseHandler NotConfirmed()
        {
            return new CommandResponseHandler("This is a protected command and needs to be confirmed.");
        }
    }
}