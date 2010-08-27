using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Vorticough : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            return new CommandResponseHandler(new Message().get("Vortigaunt"));
        }
    }
}
