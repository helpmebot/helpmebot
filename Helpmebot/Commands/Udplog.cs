using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Udplog : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            if (args.Length >= 1)
            {
                int port;
                if (int.TryParse(args[0], out port))
                {
                    Logger.instance().copyToUdp = port;
                    return new CommandResponseHandler("Set logger to udp://127.0.0.1:" + args[0]);
                }
                return new CommandResponseHandler("Not an int.");
            }
            return new CommandResponseHandler(new Message().get("notEnoughParameters"));
        }
    }
}
