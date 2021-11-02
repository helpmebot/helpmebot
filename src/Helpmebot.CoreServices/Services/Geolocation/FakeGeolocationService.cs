namespace Helpmebot.CoreServices.Services.Geolocation
{
    using System.Net;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;

    public class FakeGeolocationService : IGeolocationService
    {
        public GeolocateResult GetLocation(IPAddress address)
        {
            return new GeolocateResult { Country = "Not configured", City = "Not configured" };
        }
    }
}