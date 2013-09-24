// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializableArrayList.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Serializable arraylist
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.Collections;
    using System.Runtime.Serialization;

    /// <summary>
    /// Serializable arraylist
    /// </summary>
    [Serializable]
    internal sealed class SerializableArrayList : ArrayList, ISerializable
    {
        public SerializableArrayList(SerializationInfo info, StreamingContext ctxt)
        {
            int cnt = info.GetInt32("count");
            for (int i = 0; i < cnt; i++)
            {
                this.Add(info.GetString(i.ToString()));
            }
        }

        public SerializableArrayList()
        {
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("count", this.Count);
            for (int i = 0; i < this.Count; i++)
            {
                info.AddValue(i.ToString(), this[i].ToString());
            }
        }

        #endregion
    }
}