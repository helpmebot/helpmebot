using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Lick : Trout
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string name;
            if( args.Length == 0 ) {
                name = Message().get("cmdLickSelf");
            }
            else
            {
                name = string.Join(" ", args);
            }
            string[] messageparams = { name };
            string message = new Message().get("cmdLick", messageparams);

            return new CommandResponseHandler(message);
        }
    }
}
