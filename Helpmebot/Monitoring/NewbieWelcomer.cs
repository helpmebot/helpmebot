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

using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

#endregion

namespace helpmebot6.Monitoring
{
    /// <summary>
    /// Newbie welcomer subsystem
    /// </summary>
    internal class NewbieWelcomer
    {
        private static NewbieWelcomer _instance;

        protected NewbieWelcomer()
        {
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

        /// <summary>
        /// Executes the newbie.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="channel">The channel.</param>
        public void execute(User source, string channel)
        {
            if (Configuration.singleton()["silence",channel] == "false" &&
                Configuration.singleton()["welcomeNewbie",channel] == "true")
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
                    Helpmebot6.irc.ircPrivmsg(channel, new Message().get("WelcomeMessage-" + channel.Replace("#",""), cmdArgs));
                }
            }
        }

        /// <summary>
        /// Adds a host to the list of detected newbie hosts.
        /// </summary>
        /// <param name="host">The host.</param>
        public void addHost(string host)
        {
            this._hostNames.Add(host);

            saveHostnames();
        }

        public void delHost(string host)
        {
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
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this._hostNames);

            byte[] buf = ms.GetBuffer();

            DAL.singleton().proc_HMB_UPDATE_BINARYSTORE(buf, "newbie_hostnames");
        }
    }
}