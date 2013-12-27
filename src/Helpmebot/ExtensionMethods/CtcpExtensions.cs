// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtcpExtensions.cs" company="Helpmebot Development Team">
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
//   Defines the CtcpExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ExtensionMethods
{
    using System;
    using System.Text;

    /// <summary>
    /// The CTCP extensions.
    /// </summary>
    public static class CtcpExtensions
    {
        /// <summary>
        /// Wrap a message as a CTCP command
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="ctcpCommand">
        /// The CTCP command.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string SetupForCtcp(this string message, string ctcpCommand)
        {
            var asc = new ASCIIEncoding();
            byte[] ctcp = { Convert.ToByte(1) };
            return asc.GetString(ctcp) + ctcpCommand.ToUpper()
                   + (message == string.Empty ? string.Empty : " " + message) + asc.GetString(ctcp);
        }
    }
}
