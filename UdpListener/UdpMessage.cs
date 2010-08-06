// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
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