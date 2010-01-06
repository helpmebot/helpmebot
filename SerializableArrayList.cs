using System;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace helpmebot6
{
    [Serializable()]
    class SerializableArrayList : ArrayList,  ISerializable
    {
        public SerializableArrayList( SerializationInfo info, StreamingContext ctxt )
        {
            int cnt = info.GetInt32( "count" );
            for( int i = 0; i < cnt; i++ )
            {
                this.Add( info.GetString( i.ToString( ) ) );
            }
        }
        public SerializableArrayList( )
        {
            
        }

        #region ISerializable Members

        public void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "count", Count );
            for( int i = 0; i < Count; i++ )
            {
                info.AddValue( i.ToString(), this[ i ].ToString( ) );
            }
        }

        #endregion
    }
}
