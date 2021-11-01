namespace Helpmebot.CoreServices.Security
{
    using System.Net;
    using Castle.Windsor;
    using Helpmebot.Configuration;

    public static class TransportLayerSecurityConfigurationProvider
    {
        public static void ConfigureCertificateValidation(IWindsorContainer container)
        {
            var disableCertificateValidation = container.Resolve<BotConfiguration>().DisableCertificateValidation;

            if (disableCertificateValidation)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            }
        }
    }
}