using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Charge : Trout
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            if (args.Length > 0 && args[0] != string.Empty)
            {

                string name = args[0];
                if (GlobalFunctions.isInArray(name.ToLower(), forbiddenTargets) != -1)
                {
                    name = source.nickname;
                }
                return new CommandResponseHandler(new Message().get("cmdChargeParam", name));

            } else {
                name = source.nickname;
            }
            
            string[] messageparams = { name };
            return new CommandResponseHandler(new Message().get("cmdCharge", messageparams));
        }
    }
}
