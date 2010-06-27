#region Usings

using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands
{
    internal class Welcomer : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            switch (args[0].ToLower())
            {
                case "enable":
                    if (Configuration.singleton().retrieveLocalStringOption("welcomeNewbie", channel) == "true")
                    {
                        return new CommandResponseHandler(Configuration.singleton().getMessage("no-change"));
                    }
                    Configuration.singleton().setLocalOption("welcomeNewbie", channel, "true");
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "disable":
                    if (Configuration.singleton().retrieveLocalStringOption("welcomeNewbie", channel) == "false")
                    {
                        return new CommandResponseHandler(Configuration.singleton().getMessage("no-change"));
                    }
                    Configuration.singleton().setLocalOption("welcomeNewbie", channel, "false");
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "global":
                    Configuration.singleton().deleteLocalOption("welcomeNewbie", channel);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("defaultSetting"));
                case "add":
                    NewbieWelcomer.instance().addHost(args[1]);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "del":
                    NewbieWelcomer.instance().delHost(args[1]);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "list":
                    CommandResponseHandler crh = new CommandResponseHandler();
                    string[] list = NewbieWelcomer.instance().getHosts();
                    foreach (string item in list)
                    {
                        crh.respond(item);
                    }
                    return crh;
                case "ignore":
                    ignore( args[ 1 ] );
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "ignorelist":
                    CommandResponseHandler c = new CommandResponseHandler();
                    ArrayList nicks = getIgnored();
                    foreach (string item in nicks)
                    {
                        c.respond(item);
                    }
                    return c;
                case "unignore":
                    unignore(args[1]);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
            }
            return new CommandResponseHandler();
        }

        private static void ignore( string s )
        {
            DAL.Select q = new DAL.Select("bin_blob");
            q.setFrom("binary_store");
            q.addWhere(new DAL.WhereConds("bin_desc", "newbie_ignorednicks"));
            ArrayList result = DAL.singleton().executeSelect(q);

            byte[] list = ( (byte[])( ( (object[])( result[0] ) )[0] ) );


            BinaryFormatter bf = new BinaryFormatter();
            SerializableArrayList newbieIgnoredNicks;

            try
            {
                newbieIgnoredNicks = (SerializableArrayList)bf.Deserialize( new MemoryStream( list ) );
            }
            catch (SerializationException ex)
            {
                GlobalFunctions.errorLog(ex);
                newbieIgnoredNicks = new SerializableArrayList();
            }

            newbieIgnoredNicks.Add( s );

            bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, newbieIgnoredNicks);

            byte[] buf = ms.GetBuffer();

            DAL.singleton().proc_HMB_UPDATE_BINARYSTORE(buf, "newbie_ignorednicks");
        }

        private static void unignore(string s)
        {
            DAL.Select q = new DAL.Select("bin_blob");
            q.setFrom("binary_store");
            q.addWhere(new DAL.WhereConds("bin_desc", "newbie_ignorednicks"));
            ArrayList result = DAL.singleton().executeSelect(q);

            byte[] list = ( (byte[])( ( (object[])( result[0] ) )[0] ) );


            BinaryFormatter bf = new BinaryFormatter();
            SerializableArrayList newbieIgnoredNicks;

            try
            {
                newbieIgnoredNicks = (SerializableArrayList)bf.Deserialize(new MemoryStream(list));
            }
            catch (SerializationException ex)
            {
                GlobalFunctions.errorLog(ex);
                newbieIgnoredNicks = new SerializableArrayList();
            }

            newbieIgnoredNicks.Remove(s);

            bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, newbieIgnoredNicks);

            byte[] buf = ms.GetBuffer();

            DAL.singleton().proc_HMB_UPDATE_BINARYSTORE(buf, "newbie_ignorednicks");
        }

        static ArrayList getIgnored()
        {
            DAL.Select q = new DAL.Select("bin_blob");
            q.setFrom("binary_store");
            q.addWhere(new DAL.WhereConds("bin_desc", "newbie_ignorednicks"));
            ArrayList result = DAL.singleton().executeSelect(q);

            byte[] list = ( (byte[])( ( (object[])( result[0] ) )[0] ) );


            BinaryFormatter bf = new BinaryFormatter();

            try
            {
                return (SerializableArrayList)bf.Deserialize(new MemoryStream(list));
            }
            catch (SerializationException ex)
            {
                GlobalFunctions.errorLog(ex);
                return new SerializableArrayList();
            }


        }
    }
}