using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Uncurl command to set the bot's hedgehog status to false.
    /// </summary>
    /// <remarks>This is a fun command, but because FunCommand checks hedgehog is false, that base class can't be used.</remarks>
    class Uncurl : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Configuration.singleton()["hedgehog", channel] = "false";
            return new CommandResponseHandler(new Message().get("Done"));
        }
    }
}
