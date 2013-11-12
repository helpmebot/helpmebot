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

namespace Helpmebot
{
    using System;

    using Helpmebot.Legacy.Database;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The message.
    /// </summary>
    internal class Message
    {
        /// <summary>
        /// The database access layer.
        /// </summary>
        private readonly DAL database;

        /// <summary>
        /// The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <remarks>
        /// Legacy code.
        /// </remarks>
        public Message()
        {
            this.database = DAL.singleton();
            this.messageService = Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<IMessageService>();
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="messageName">
        /// The message name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [Obsolete("Use message service instead")]
        public string GetMessage(string messageName)
        {
            return this.messageService.RetrieveMessage(messageName, null);
        }

        /// <summary>
        /// The get message.
        /// </summary>
        /// <param name="messageName">
        /// The message name.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [Obsolete("Use message service instead")]
        public string GetMessage(string messageName, params string[] args)
        {
            return this.messageService.RetrieveMessage(messageName, args);
        }
    }
}
