using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands.FunStuff
{
    abstract class FunCommand : GenericCommand
    {
        protected override CommandResponseHandler accessDenied(User source, string channel, string[] args)
        {
            return Configuration.singleton()["hedgehog", channel] == "false" ? 
                base.accessDenied(source, channel, args) : 
                new CommandResponseHandler(new Message().get("HedgehogAccessDenied"),CommandResponseDestination.PrivateMessage);
        }

        protected override bool accessTest(User source, string channel)
        {
            return Configuration.singleton()["hedgehog", channel] == "false" && base.accessTest(source, channel);
        }
    }
}
