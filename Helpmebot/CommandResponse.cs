// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandResponse.cs" company="Helpmebot Development Team">
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
//   Command response destinations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6
{
    using System.Collections;

    /// <summary>
    /// Command response destinations
    /// </summary>
    public enum CommandResponseDestination
    {
        /// <summary>
        /// Back to the channel from whence it came
        /// </summary>
        Default,
        /// <summary>
        /// To the debugging channel
        /// </summary>
        ChannelDebug,
        /// <summary>
        /// Back to the user in a private message
        /// </summary>
        PrivateMessage
    }

    /// <summary>
    /// The individual response
    /// </summary>
    internal struct CommandResponse
    {
        public CommandResponseDestination destination;
        public string message;
    }

    /// <summary>
    /// Holds the response to a command
    /// </summary>
    public class CommandResponseHandler
    {
        private readonly ArrayList _responses;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandResponseHandler"/> class.
        /// </summary>
        public CommandResponseHandler()
        {
            this._responses = new ArrayList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandResponseHandler"/> class.
        /// </summary>
        /// <param name="message">pre-respond with this message.</param>
        public CommandResponseHandler(string message)
        {
            this._responses = new ArrayList();
            respond(message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandResponseHandler"/> class.
        /// </summary>
        /// <param name="message">A message.</param>
        /// <param name="destination">The destination of the message.</param>
        public CommandResponseHandler(string message, CommandResponseDestination destination)
        {
            this._responses = new ArrayList();
            respond(message, destination);
        }

        /// <summary>
        /// Adds the specified message to the response.
        /// </summary>
        /// <param name="message">The message.</param>
        public void respond(string message)
        {
            CommandResponse cr;
            cr.destination = CommandResponseDestination.Default;
            cr.message = message;

            this._responses.Add(cr);
        }

        /// <summary>
        /// Adds the specified message to the response.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="destination">The destination.</param>
        public void respond(string message, CommandResponseDestination destination)
        {

            CommandResponse cr;
            cr.destination = destination;
            cr.message = message;

            this._responses.Add(cr);
        }

        /// <summary>
        /// Appends the specified more responses.
        /// </summary>
        /// <param name="moreResponses">The more responses.</param>
        public void append(CommandResponseHandler moreResponses)
        {
            foreach (object item in moreResponses.getResponses())
            {
                this._responses.Add(item);
            }
        }

        /// <summary>
        /// Gets the responses.
        /// </summary>
        /// <returns></returns>
        public ArrayList getResponses()
        {
            return this._responses;
        }
    }
}