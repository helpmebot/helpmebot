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

namespace Helpmebot.IRC.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Helpmebot.ExtensionMethods;

    /// <summary>
    /// The message.
    /// </summary>
    public class Message : IMessage
    {
        /// <summary>
        /// The command.
        /// </summary>
        private readonly string command;

        /// <summary>
        /// The prefix.
        /// </summary>
        private readonly string prefix;

        /// <summary>
        /// The parameters.
        /// </summary>
        private readonly IEnumerable<string> parameters;

        /// <summary>
        /// Initialises a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        public Message(string command)
            : this(null, command, null)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameter">
        /// The parameters.
        /// </param>
        public Message(string command, string parameter)
            : this(null, command, parameter.ToEnumerable())
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        public Message(string command, IEnumerable<string> parameters)
            : this(null, command, parameters)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="prefix">
        /// The prefix.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        public Message(string prefix, string command, IEnumerable<string> parameters)
        {
            this.prefix = prefix;
            this.command = command;
            this.parameters = parameters;
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public string Command
        {
            get
            {
                return this.command;
            }
        }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public IEnumerable<string> Parameters
        {
            get
            {
                return this.parameters == null ? null : this.parameters.ToArray();
            }
        }

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
            var separator = new[] { ' ' };
            string prefix = null, command;
            List<string> messageParameters = null;

            if (data.StartsWith(":"))
            {
                var prefixstrings = data.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
                data = prefixstrings[1];
                prefix = prefixstrings[0].Substring(1); // strip the leading : too
            }

            var strings = data.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
            command = strings[0];

            if (strings.Length == 2)
            {
                var parameters = strings[1];

                if (parameters.Contains(" :") || parameters.StartsWith(":"))
                {
                    var paramend = parameters.Substring(parameters.IndexOf(":", StringComparison.Ordinal) + 1);
                    var parameterList =
                        parameters.Substring(0, parameters.IndexOf(":", StringComparison.Ordinal))
                            .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();

                    parameterList.Add(paramend);
                    messageParameters = parameterList;
                }
                else
                {
                    messageParameters = parameters.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }

            return new Message(prefix, command, messageParameters);
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
