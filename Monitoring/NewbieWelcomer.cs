#region Usings

using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

#endregion

namespace helpmebot6.Monitoring
{
    internal class NewbieWelcomer
    {
        private static NewbieWelcomer _instance;

        protected NewbieWelcomer()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            DAL.Select q = new DAL.Select("bin_blob");
            q.setFrom("binary_store");
            q.addWhere(new DAL.WhereConds("bin_desc", "newbie_hostnames"));
            ArrayList result = DAL.singleton().executeSelect(q);

            byte[] list = ((byte[]) (((object[]) (result[0]))[0]));


            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                this._hostNames = (SerializableArrayList) bf.Deserialize(new MemoryStream(list));
            }
            catch (SerializationException ex)
            {
                GlobalFunctions.errorLog(ex);
                this._hostNames = new SerializableArrayList();
            }
        }

        public static NewbieWelcomer instance()
        {
            return _instance ?? ( _instance = new NewbieWelcomer( ) );
        }

        private readonly SerializableArrayList _hostNames;

        public void execute(User source, string channel)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (Configuration.singleton().retrieveLocalStringOption("silence", channel) == "false" &&
                Configuration.singleton().retrieveLocalStringOption("welcomeNewbie", channel) == "true")
            {
                bool match = false;
                foreach (object item in this._hostNames)
                {
                    string pattern = (string) item;
                    Regex rX = new Regex(pattern);
                    if (rX.IsMatch(source.hostname))
                    {
                        match = true;
                        break;
                    }
                }

                if (match)
                {
                    string[] cmdArgs = {source.nickname, channel};
                    Helpmebot6.irc.ircPrivmsg(channel, Configuration.singleton().getMessage("welcomeMessage", cmdArgs));
                }
            }
        }

        public void addHost(string host)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            this._hostNames.Add(host);

            saveHostnames();
        }

        public void delHost(string host)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            this._hostNames.Remove(host);

            saveHostnames();
        }

        public string[] getHosts()
        {
            string[] list = new string[this._hostNames.Count];
            this._hostNames.CopyTo(list);
            return list;
        }

        private void saveHostnames()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this._hostNames);

            byte[] buf = ms.GetBuffer();

            DAL.singleton().proc_HMB_UPDATE_BINARYSTORE(buf, "newbie_hostnames");
        }
    }
}