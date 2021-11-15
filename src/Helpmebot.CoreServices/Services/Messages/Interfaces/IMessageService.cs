// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageService.cs" company="Helpmebot Development Team">
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
//   Defines the IMessageService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.CoreServices.Services.Messages.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// The MessageService interface.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Retrieve a message from a message source with context.
        /// </summary>
        /// <param name="messageKey">
        /// The message key.
        /// </param>
        /// <param name="context">
        /// The context for retrieving a message.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// The message
        /// </returns>
        string RetrieveMessage(string messageKey, object context, IEnumerable<string> arguments);

        /// <summary>
        /// Refreshes the <see cref="Helpmebot.Repositories.ResponseRepository">ResponseRepository</see> from the
        /// database.
        /// </summary>
        void RefreshResponseRepository();
    }
}
