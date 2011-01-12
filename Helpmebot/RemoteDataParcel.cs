using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace helpmebot6
{
    [Serializable]
    public class RemoteDataParcel : ISerializable
    {
        public static string Serialize(RemoteDataParcel dataParcel)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            bf.Serialize(ms, dataParcel);

            ms.Seek(0, SeekOrigin.Begin);

            byte[] b = ms.ToArray();
            string s = Convert.ToBase64String(b);
            return s;
        }

        public static RemoteDataParcel Unserialize(string dataParcel)
        {
            BinaryFormatter bf = new BinaryFormatter();

            byte[] b = Convert.FromBase64String(dataParcel);
            MemoryStream ms = new MemoryStream(b);
            return (RemoteDataParcel)bf.Deserialize(ms);
        }

        private int _colourParameter;
        public int colourParameter
        {
            get { return _colourParameter; }
            set { _colourParameter = value; }
        }

        private int _motionParameter;
        public int motionParameter
        {
            get { return _motionParameter; }
            set { _motionParameter = value; }
        }

        private int _timerInterval;
        public int timerInterval
        {
            get { return _timerInterval; }
            set { _timerInterval = value; }
        }

        private int _colourIndex;
        public int colourIndex
        {
            get { return _colourIndex; }
            set { _colourIndex = value; }
        }

        private int _motionIndex;
        public int motionIndex
        {
            get { return _motionIndex; }
            set { _motionIndex = value; }
        }

        public RemoteDataParcel()
        {

        }

        public RemoteDataParcel(SerializationInfo info, StreamingContext ctxt)
        {
            _colourParameter = info.GetInt32("colourParameter");
            _motionParameter = info.GetInt32("motionParameter");
            _timerInterval = info.GetInt32("timerInterval");
            _colourIndex = info.GetInt32("colourIndex");
            _motionIndex = info.GetInt32("motionIndex");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("colourParameter", _colourParameter);
            info.AddValue("motionParameter", _motionParameter);
            info.AddValue("timerInterval", _timerInterval);
            info.AddValue("colourIndex", _colourIndex);
            info.AddValue("motionIndex", _motionIndex);
        }
    }
}
