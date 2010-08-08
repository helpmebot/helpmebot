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
using System.Collections;
using System.Runtime.Serialization;

#endregion

namespace helpmebot6
{
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
                Add(info.GetString(i.ToString()));
            }
        }

        public SerializableArrayList()
        {
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("count", Count);
            for (int i = 0; i < Count; i++)
            {
                info.AddValue(i.ToString(), this[i].ToString());
            }
        }

        #endregion
    }
}