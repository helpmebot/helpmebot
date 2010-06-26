#region Usings

using System;
using System.Collections;
using System.Runtime.Serialization;

#endregion

namespace helpmebot6
{
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