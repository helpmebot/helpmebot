using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Accmon : GenericCommand
    {
        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            return new CommandResponseHandler(Monitoring.AccNotifications.getInstance().getThreadStatus()[0]);
        }

        #endregion
    }
}
