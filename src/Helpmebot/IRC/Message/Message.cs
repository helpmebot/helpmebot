// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Message.cs" company="Helpmebot Development Team">
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
//   Defines the Message type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Message
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The message.
    /// </summary>
    public class Message : IMessage
    {
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public IEnumerable<string> Parameters { get; set; }

        /// <summary>
        /// The parse.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="IMessage"/>.
        /// </returns>
        public static IMessage Parse(string data)
        {
            var message = new Message();
            var separator = new[] { ' ' };

            if (data.StartsWith(":"))
            {
                var prefixstrings = data.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
                data = prefixstrings[1];
                message.Prefix = prefixstrings[0].Substring(1); // strip the leading : too
            }

            var strings = data.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
            message.Command = strings[0];

            if (strings.Length == 2)
            {
                var parameters = strings[1];

                if (parameters.Contains(":"))
                {
                    var paramend = parameters.Substring(parameters.IndexOf(":", StringComparison.Ordinal) + 1);
                    var parameterList =
                        parameters.Substring(0, parameters.IndexOf(":", StringComparison.Ordinal))
                            .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();

                    parameterList.Add(paramend);
                    message.Parameters = parameterList;
                }
                else
                {
                    var parameterList =
                        parameters.Substring(0, parameters.IndexOf(":", StringComparison.Ordinal))
                            .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    message.Parameters = parameterList;
                }
            }

            return message;
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(this.Prefix))
            {
                result += ":" + this.Prefix + " ";
            }

            result += this.Command;

            foreach (var p in this.Parameters)
            {
                if (p.Contains(" "))
                {
                    result += " :" + p;
                }
                else
                {
                    result += " " + p;
                }
            }

            return result;
        }
    }
}
