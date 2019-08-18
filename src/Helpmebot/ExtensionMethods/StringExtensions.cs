// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Helpmebot Development Team">
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
//   Defines the StringExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ExtensionMethods
{
    using System;
    using System.Net;

    public static class StringExtensions
    {
        public static IPAddress GetIpAddressFromHex(this string input)
        {
            var ip = new byte[4];
            ip[0] = Convert.ToByte(input.Substring(0, 2), 16);
            ip[1] = Convert.ToByte(input.Substring(2, 2), 16);
            ip[2] = Convert.ToByte(input.Substring(4, 2), 16);
            ip[3] = Convert.ToByte(input.Substring(6, 2), 16);

            var ipAddr = new IPAddress(ip);
            return ipAddr;
        }
    }
}
