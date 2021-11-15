// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageService.cs" company="Helpmebot Development Team">
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

namespace Helpmebot.CoreServices.Services.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.IrcClient.Extensions;

    public class MessageService : IMessageService
    {
        private readonly Random random;
        private readonly object randomLock = new object();

        private readonly ILogger log;
        private readonly ILegacyMessageBackend legacyMessageBackend;

        public MessageService(ILogger log, ILegacyMessageBackend legacyMessageBackend)
        {
            this.log = log;
            this.legacyMessageBackend = legacyMessageBackend;
            this.random = new Random();
        }

        public string RetrieveMessage(string messageKey, object context, IEnumerable<string> arguments)
        {
            if (string.IsNullOrEmpty(messageKey))
            {
                throw new ArgumentNullException("messageKey");
            }

            string contextData = context != null ? context.ToString() : string.Empty;

            if (!string.IsNullOrEmpty(contextData))
            {
                contextData = string.Format("/{0}", contextData);

                contextData = contextData.Replace("#", string.Empty) // will cause issues
                    .Replace("|", string.Empty) // link syntax
                    .Replace("[", string.Empty) // link syntax
                    .Replace("]", string.Empty) // link syntax
                    .Replace("{", string.Empty) // link syntax
                    .Replace("}", string.Empty) // link syntax
                    .Replace("<", string.Empty) // html issues
                    .Replace(">", string.Empty); // html issues

                return this.RetrieveMessage(messageKey, contextData, arguments);
            }

            return this.RetrieveMessage(messageKey, string.Empty, arguments);
        }

        public void RefreshResponseRepository()
        {
            this.legacyMessageBackend.RefreshResponseRepository();
        }

        /// <summary>
        /// Gets a context-sensitive message from the database.
        /// </summary>
        /// <returns>
        /// A list of messages.
        /// </returns>
        private IEnumerable<string> GetMessageFromDatabase(string messageKey, string contextPath)
        {
            // attempt to get some context-sensitive message.
            var results = this.GetRawMessageFromDatabase(string.Concat(messageKey, contextPath)).ToList();

            if (!results.Any())
            {
                // nothing found, fall back on the value with no context.
                this.log.InfoFormat(
                    "Message {0} with context path {1} not found: Falling back to non-context-sensitive message.",
                    messageKey,
                    contextPath);

                results = this.GetRawMessageFromDatabase(messageKey).ToList();
            }

            if (results.Any())
            {
                return results;
            }

            this.log.ErrorFormat("Message {0} not found.", messageKey);
            return null;
        }

        /// <summary>
        /// Gets a raw message key (including any context path) from the database
        /// </summary>
        /// <returns>
        /// A list of messages.
        /// </returns>
        private IEnumerable<string> GetRawMessageFromDatabase(string messageKey)
        {
            return this.legacyMessageBackend.GetRawMessages(messageKey);
        }

        private string RetrieveMessage(string messageKey, string contextPath, IEnumerable<string> arguments)
        {
            // normalise message name to account for old messages
            if (messageKey.Substring(0, 1).ToUpper() != messageKey.Substring(0, 1))
            {
                messageKey = messageKey.Substring(0, 1).ToUpper() + messageKey.Substring(1);
            }

            var messageFromDatabase = this.GetMessageFromDatabase(messageKey, contextPath);

            if (messageFromDatabase == null)
            {
                return null;
            }

            List<string> messages = messageFromDatabase.ToList();

            // let's grab a random message from the tin:
            int randomNumber;
            lock (this.randomLock)
            {
                randomNumber = this.random.Next(0, messages.Count());
            }

            string builtString = messages[randomNumber];

            if (arguments != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                object[] args = arguments.ToArray();
                builtString = string.Format(builtString, args);
            }

            if (builtString.StartsWith("#ACTION"))
            {
                builtString = builtString.Substring(8).SetupForCtcp("ACTION");
            }

            return builtString;
        }
    }
}
