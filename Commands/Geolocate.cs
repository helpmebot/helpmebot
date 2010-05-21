using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml;

namespace helpmebot6.Commands
{
    class Geolocate : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            GeolocateResult location = getLocation( IPAddress.Parse( args[ 1 ] ) );
            string[ ] messageArgs = { location.ToString( ) };
            return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "locationMessage", messageArgs ) );
        }

        public static GeolocateResult getLocation( IPAddress ip )
        {
            XmlTextReader xtr = new XmlTextReader( HttpRequest.get( "http://ipinfodb.com/ip_query.php?timezone=false&ip=" + ip.ToString( ) ) );

            GeolocateResult result = new GeolocateResult();

            while( !xtr.EOF )
            {
                switch( xtr.Name )
                {
                    case "Status":
                        result.Status = xtr.ReadContentAsString( );
                        break;
                    case "CountryCode":
                        result.CountryCode = xtr.ReadContentAsString( );
                        break;
                    case "CountryName":
                        result.Country = xtr.ReadContentAsString( );
                        break;
                    case "RegionCode":
                        result.RegionCode = xtr.ReadContentAsString( );
                        break;
                    case "RegionName":
                        result.Region = xtr.ReadContentAsString( );
                        break;
                    case "City":
                        result.City = xtr.ReadContentAsString( );
                        break;
                    case "ZipPostalCode":
                        result.ZipPostalCode = xtr.ReadContentAsString( );
                        break;
                    case "Latitude":
                        result.Latitude = xtr.ReadContentAsFloat( );
                        break;
                    case "Longitude":
                        result.Longitude = xtr.ReadContentAsFloat( );
                        break;
                }
            }
            return result;

        }

        public struct GeolocateResult
        {
           public string Status, CountryCode, Country, RegionCode, Region, City, ZipPostalCode;
           public float Latitude, Longitude;

           public override string ToString( )
           {
               return City + ", " + Region + ", " + Country;
           }
        }
    }
}
