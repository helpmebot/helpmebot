#region Usings

using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;

#endregion

namespace helpmebot6.Commands
{
    internal class Geolocate : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            GeolocateResult location = getLocation(IPAddress.Parse(args[0]));
            string[] messageArgs = {location.ToString()};
            return new CommandResponseHandler(Configuration.singleton().getMessage("locationMessage", messageArgs));
        }

        public static GeolocateResult getLocation(IPAddress ip)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            Stream s = HttpRequest.get("http://ipinfodb.com/ip_query.php?timezone=false&ip=" + ip);
            XmlTextReader xtr = new XmlTextReader(s);
            GeolocateResult result = new GeolocateResult();

            while (!xtr.EOF)
            {
                xtr.Read();
                switch (xtr.Name)
                {
                    case "Status":
                        result.status = xtr.ReadElementContentAsString();
                        break;
                    case "CountryCode":
                        result.countryCode = xtr.ReadElementContentAsString();
                        break;
                    case "CountryName":
                        result.country = xtr.ReadElementContentAsString();
                        break;
                    case "RegionCode":
                        result.regionCode = xtr.ReadElementContentAsString();
                        break;
                    case "RegionName":
                        result.region = xtr.ReadElementContentAsString();
                        break;
                    case "City":
                        result.city = xtr.ReadElementContentAsString();
                        break;
                    case "ZipPostalCode":
                        result.zipPostalCode = xtr.ReadElementContentAsString();
                        break;
                    case "Latitude":
                        result.latitude = xtr.ReadElementContentAsFloat();
                        break;
                    case "Longitude":
                        result.longitude = xtr.ReadElementContentAsFloat();
                        break;
                }
            }
            return result;
        }

        public struct GeolocateResult
        {
            public string status;
            public string countryCode;
            public string country;
            public string regionCode;
            public string region;
            public string city;
            public string zipPostalCode;
            public float latitude;
            public float longitude;

            public override string ToString()
            {
                Logger.instance().addToLog(
                    "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                    Logger.LogTypes.DNWB);

                return this.city + ", " + this.region + ", " + this.country;
            }
        }
    }
}