#region Usings

using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace helpmebot6.UdpListener
{
    [Serializable]
    public class UdpMessage : ISerializable
    {
        public string hash { get; private set; }

        public string message { get; private set; }

        public UdpMessage(string message, string key)
        {
            this.message = message;
            byte[] bm = Encoding.ASCII.GetBytes(message + key);
            byte[] bh = MD5.Create().ComputeHash(bm);
            this.hash = Encoding.ASCII.GetString(bh);
        }

        #region ISerializable Members

        public UdpMessage(SerializationInfo info, StreamingContext ctxt)
        {
            this.hash = info.GetString("hash");
            this.message = info.GetString("message");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("hash", this.hash);
            info.AddValue("message", this.message);
        }

        #endregion
    }
}