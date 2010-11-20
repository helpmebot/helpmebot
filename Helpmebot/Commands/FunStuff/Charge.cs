using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Charge : FunStuff.FunCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            if (args.Length > 0)
                return new CommandResponseHandler(IAL.wrapCTCP("ACTION", new Message().get("cmdChargeParam", args[0])));
            return new CommandResponseHandler(IAL.wrapCTCP("ACTION", new Message().get("cmdCharge")));
        }
    }
}
