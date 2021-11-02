namespace Helpmebot.CoreServices.Security
{
    using System.Net;

    public static class TransportLayerSecurityConfigurationProvider
    {
        public static void ConfigureCertificateValidation(bool certificateValidation)
        {
            if (certificateValidation)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            }
        }
    }
}