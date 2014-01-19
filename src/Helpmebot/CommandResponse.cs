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
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot
{
    using System.Collections;

    /// <summary>
    ///     Command response destinations
    /// </summary>
    public enum CommandResponseDestination
    {
        /// <summary>
        ///     Back to the channel from whence it came
        /// </summary>
        Default, 

        /// <summary>
        ///     To the debugging channel
        /// </summary>
        ChannelDebug, 

        /// <summary>
        ///     Back to the user in a private message
        /// </summary>
        PrivateMessage
    }

    /// <summary>
    ///     The individual response
    /// </summary>
    public struct CommandResponse
    {
        #region Fields

        /// <summary>
        /// The destination.
        /// </summary>
        public CommandResponseDestination Destination;

        /// <summary>
        /// The message.
        /// </summary>
        public string Message;

        #endregion
    }

    /// <summary>
    ///     Holds the response to a command
    /// </summary>
    public class CommandResponseHandler
    {
        #region Fields

        /// <summary>
        /// The _responses.
        /// </summary>
        private readonly ArrayList responses;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandResponseHandler"/> class. 
        /// </summary>
        public CommandResponseHandler()
        {
            this.responses = new ArrayList();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandResponseHandler"/> class. 
        /// </summary>
        /// <param name="message">
        /// pre-respond with this message.
        /// </param>
        public CommandResponseHandler(string message)
        {
            this.responses = new ArrayList();
            this.Respond(message);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandResponseHandler"/> class. 
        /// </summary>
        /// <param name="message">
        /// A message.
        /// </param>
        /// <param name="destination">
        /// The destination of the message.
        /// </param>
        public CommandResponseHandler(string message, CommandResponseDestination destination)
        {
            this.responses = new ArrayList();
            this.Respond(message, destination);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Appends the specified more responses.
        /// </summary>
        /// <param name="moreResponses">
        /// The more responses.
        /// </param>
        public void Append(CommandResponseHandler moreResponses)
        {
            foreach (object item in moreResponses.GetResponses())
            {
                this.responses.Add(item);
            }
        }

        /// <summary>
        /// Gets the responses.
        /// </summary>
        /// <returns>
        /// The <see cref="ArrayList"/>.
        /// </returns>
        public ArrayList GetResponses()
        {
            return this.responses;
        }

        /// <summary>
        /// Adds the specified message to the response.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Respond(string message)
        {
            CommandResponse cr;
            cr.Destination = CommandResponseDestination.Default;
            cr.Message = message;

            this.responses.Add(cr);
        }

        /// <summary>
        /// Adds the specified message to the response.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        public void Respond(string message, CommandResponseDestination destination)
        {
            CommandResponse cr;
            cr.Destination = destination;
            cr.Message = message;

            this.responses.Add(cr);
        }

        #endregion
    }
}