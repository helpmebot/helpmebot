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
// <summary>
//   Defines the MessageService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Castle.Core.Logging;

    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The message service.
    /// </summary>
    public class MessageService : IMessageService
    {
        /// <summary>
        /// The legacy database.
        /// </summary>
        private readonly IDAL legacyDatabase;

        /// <summary>
        /// The random generator.
        /// </summary>
        private readonly Random random;

        /// <summary>
        /// The random lock.
        /// </summary>
        private readonly object randomLock = new object();

        /// <summary>
        /// Initialises a new instance of the <see cref="MessageService"/> class.
        /// </summary>
        /// <param name="legacyDatabase">
        /// The legacy database.
        /// </param>
        public MessageService(IDAL legacyDatabase)
        {
            this.legacyDatabase = legacyDatabase;
            this.random = new Random();
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        /// The retrieve message.
        /// </summary>
        /// <param name="messageKey">
        /// The message key.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string RetrieveMessage(string messageKey, object context, IEnumerable<string> arguments)
        {
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

        /// <summary>
        /// The retrieve message.
        /// </summary>
        /// <param name="messageKey">
        /// The message key.
        /// </param>
        /// <param name="contextPath">
        /// The context path.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string RetrieveMessage(string messageKey, string contextPath, IEnumerable<string> arguments)
        {
            // normalise message name to account for old messages
            if (messageKey.Substring(0, 1).ToUpper() != messageKey.Substring(0, 1))
            {
                messageKey = messageKey.Substring(0, 1).ToUpper() + messageKey.Substring(1);
            }

            var messages = this.GetMessageFromDatabase(messageKey, contextPath).ToList();

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

        /// <summary>
        /// Gets a context-sensitive message from the database.
        /// </summary>
        /// <param name="messageKey">
        /// The message key.
        /// </param>
        /// <param name="contextPath">
        /// The context path.
        /// </param>
        /// <returns>
        /// The list of messages.
        /// </returns>
        private IEnumerable<string> GetMessageFromDatabase(string messageKey, string contextPath)
        {
            // attempt to get some context-sensitive message.
            var results = this.GetRawMessageFromDatabase(string.Concat(messageKey, contextPath)).ToList();

            if (!results.Any())
            {
                // nothing found, fall back on the value with no context.
                results = this.GetRawMessageFromDatabase(messageKey).ToList();

                this.Log.InfoFormat("Message {0} with context path {1} not found: Falling back to non-context-sensitive message.", messageKey, contextPath);
            }

            if (results.Any())
            {
                return results;
            }

            this.Log.ErrorFormat("Message {0} not found.", messageKey);
            return null;
        }

        /// <summary>
        /// Gets a raw message key (including any context path) from the database
        /// </summary>
        /// <param name="messageKey">
        /// The message key.
        /// </param>
        /// <returns>
        /// The list of messages.
        /// </returns>
        private IEnumerable<string> GetRawMessageFromDatabase(string messageKey)
        {
            DAL.Select query =
                new DAL.Select("message_text").From("messages").Where(new DAL.WhereConds("message_name", messageKey));
            var result = this.legacyDatabase.executeSelect(query);

            if (result.Count == 1)
            {
                // extract the byte array from the dataset
                var y = (byte[])((object[])result[0])[0];

                Encoding encoding = Encoding.UTF8;
                return encoding.GetString(y).Split('\n').ToList();
            }

            return new List<string>();
        }
    }
}
