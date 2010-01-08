using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
namespace helpmebot6.UdpListener
{
    [Serializable()]
    public class  UdpMessage : ISerializable
    {
        public string Hash
        {
            get
            {
                return _hash;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        string _hash;
        string _message;

        public UdpMessage(string message, string key )
        {
            _message = message;
            byte[ ] bm = ASCIIEncoding.ASCII.GetBytes( message + key );
            byte[ ] bh = MD5.Create( ).ComputeHash( bm );
            _hash = ASCIIEncoding.ASCII.GetString( bh );
        }

        #region ISerializable Members

        public UdpMessage( SerializationInfo info, StreamingContext ctxt )
        {
            _hash = info.GetString( "hash" );
            _message = info.GetString( "message" );
        }

        public void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "hash", _hash );
            info.AddValue( "message", _message );
        }

        #endregion
    }
}
