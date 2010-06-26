#region Usings

using System.Net;
using System.Net.Sockets;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Resolve : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            IPAddress[] addresses = new IPAddress[0];
            try
            {
                addresses = Dns.GetHostEntry(args[0]).AddressList;
            }
            catch (SocketException)
            {
            }
            if (addresses.Length != 0)
            {
                string ipList = "";
                bool first = true;
                foreach (IPAddress item in addresses)
                {
                    if (!first)
                        ipList += ", ";
                    ipList += item.ToString();
                    first = false;
                }
                string[] messageargs = {args[0], ipList};

                return new CommandResponseHandler(Configuration.singleton().getMessage("resolve", messageargs));
            }
            else
            {
                string[] messageargs = {args[0]};
                return new CommandResponseHandler(Configuration.singleton().getMessage("resolveFail", messageargs));
            }
        }
    }
}