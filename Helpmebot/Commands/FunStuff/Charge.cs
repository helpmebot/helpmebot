// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Charge.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Defines the Charge type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    /// <summary>
    /// The charge.
    /// </summary>
    internal class Charge : Trout
    {
        /// <summary>
        /// The execute command.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            string name;
            if (args.Length > 0 && args[0] != string.Empty)
            {

                name = args[0];
                if (GlobalFunctions.isInArray(name.ToLower(), this.forbiddenTargets) != -1)
                {
                    name = source.nickname;
                }

                return new CommandResponseHandler(new Message().get("cmdChargeParam", name));
            }
            
            name = source.nickname;

            string[] messageparams = { name };
            return new CommandResponseHandler(new Message().get("cmdCharge", messageparams));
        }
    }
}
