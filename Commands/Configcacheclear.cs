using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Configcacheclear : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Configuration.singleton().clearCache();
            return new CommandResponseHandler(new Message().get("done"));
        }
    }
}
