#region Usings

using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Decode : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (args[0].Length != 8)
                return null;

            byte[] ip = new byte[4];
            ip[0] = Convert.ToByte(args[0].Substring(0, 2), 16);
            ip[1] = Convert.ToByte(args[0].Substring(2, 2), 16);
            ip[2] = Convert.ToByte(args[0].Substring(4, 2), 16);
            ip[3] = Convert.ToByte(args[0].Substring(6, 2), 16);

            IPAddress ipAddr = new IPAddress(ip);

            string hostname = "";
            try
            {
                hostname = Dns.GetHostEntry(ipAddr).HostName;
            }
            catch (SocketException)
            {
            }
            if (hostname != string.Empty)
            {
                string[] messageargs = {args[0], ipAddr.ToString(), hostname};
                return new CommandResponseHandler(Configuration.singleton().getMessage("hexDecodeResult", messageargs));
            }
            else
            {
                string[] messageargs = {args[0], ipAddr.ToString()};
                return
                    new CommandResponseHandler(Configuration.singleton().getMessage("hexDecodeResultNoResolve",
                                                                                    messageargs));
            }
        }
    }
}