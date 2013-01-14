using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace helpmebot6
{
    internal class BinaryStore
    {
        public static SerializableArrayList retrieve(string blobName)
        {
            var q = new DAL.Select("bin_blob");
            q.setFrom("binary_store");
            q.addWhere(new DAL.WhereConds("bin_desc", blobName));
            var result = DAL.singleton().executeSelect(q);
// TODO: test memstream isn't empty or catch exception

            return (SerializableArrayList)new BinaryFormatter().Deserialize(new MemoryStream(((byte[])(((object[])(result[0]))[0]))));
        }

        public static void storeValue(string hostnames, SerializableArrayList toStore)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, toStore);
            byte[] buf = ms.GetBuffer();

            DAL.singleton().proc_HMB_UPDATE_BINARYSTORE(buf, hostnames);
        }
    }
}
