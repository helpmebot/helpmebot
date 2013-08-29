using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Beer : FunStuff.FunCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string name;
            if( args.Length == 0 ) {
                name = source.nickname;
            }
            else
            {
                name = string.Join(" ", args);
            }
            string[] messageparams = { name };
            string message = new Message().get("cmdBeer", messageparams);

            return new CommandResponseHandler(message);
        }
    }
}
