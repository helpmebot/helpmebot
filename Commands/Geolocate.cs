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
            GeolocateResult location = getLocation( IPAddress.Parse( args[ 0 ] ) );
            string[ ] messageArgs = { location.ToString( ) };
            return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "locationMessage", messageArgs ) );
        }

        public static GeolocateResult getLocation( IPAddress ip )
        {
         System.IO.Stream s =   HttpRequest.get( "http://ipinfodb.com/ip_query.php?timezone=false&ip=" + ip.ToString( ) );
         XmlTextReader xtr = new XmlTextReader( s );
            GeolocateResult result = new GeolocateResult();

            while( !xtr.EOF )
            {
                xtr.Read( );
                switch( xtr.Name )
                {
                    case "Status":
                        result.Status = xtr.ReadElementContentAsString( );
                        break;
                    case "CountryCode":
                        result.CountryCode = xtr.ReadElementContentAsString( );
                        break;
                    case "CountryName":
                        result.Country = xtr.ReadElementContentAsString( );
                        break;
                    case "RegionCode":
                        result.RegionCode = xtr.ReadElementContentAsString( );
                        break;
                    case "RegionName":
                        result.Region = xtr.ReadElementContentAsString( );
                        break;
                    case "City":
                        result.City = xtr.ReadElementContentAsString( );
                        break;
                    case "ZipPostalCode":
                        result.ZipPostalCode = xtr.ReadElementContentAsString( );
                        break;
                    case "Latitude":
                        result.Latitude = xtr.ReadElementContentAsFloat( );
                        break;
                    case "Longitude":
                        result.Longitude = xtr.ReadElementContentAsFloat( );
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
