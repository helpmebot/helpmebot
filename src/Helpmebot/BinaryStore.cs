// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryStore.cs" company="Helpmebot Development Team">
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
//   Defines the BinaryStore type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.CodeDom;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;

    using Helpmebot.Legacy.Database;

    using log4net;

    internal class BinaryStore
    {
        /// <summary>
        /// The log4net logger for this class
        /// </summary>
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static SerializableArrayList retrieve(string blobName)
        {
            var q = new DAL.Select("bin_blob");
            q.setFrom("binary_store");
            q.addWhere(new DAL.WhereConds("bin_desc", blobName));
            var result = DAL.singleton().executeSelect(q);

            var serializationStream = new MemoryStream(((byte[]) (((object[]) (result[0]))[0])));
            if (serializationStream.Length != 0)
            {
                try
                {
                    return (SerializableArrayList)new BinaryFormatter().Deserialize(serializationStream);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
            }

            return new SerializableArrayList();
        }

        public static void storeValue(string hostnames, SerializableArrayList toStore)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, toStore);
            var buf = ms.GetBuffer();

            DAL.singleton().proc_HMB_UPDATE_BINARYSTORE(buf, hostnames);
        }
    }
}
