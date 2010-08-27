using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Curl : FunStuff.FunCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Configuration.singleton()["hedgehog", channel] = "true";
            return new CommandResponseHandler(new Message().get("HedgehogCurlup"));
        }
    }
}
