using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

            var serializationStream = new MemoryStream(((byte[]) (((object[]) (result[0]))[0])));
            if (serializationStream.Length != 0)
            {
                return (SerializableArrayList) new BinaryFormatter().Deserialize(serializationStream);
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
