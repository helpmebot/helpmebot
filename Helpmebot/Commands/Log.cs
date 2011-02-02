using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Log : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            DAL.singleton().insert("adminlog", "", "", source.nickname, string.Join(" ", args));
            return new CommandResponseHandler("Logged the message, Master");
        }
    }
}
