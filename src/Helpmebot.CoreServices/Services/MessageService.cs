﻿// --------------------------------------------------------------------------------------------------------------------
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

namespace Helpmebot.CoreServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.IrcClient.Extensions;

    public class MessageService : IMessageService
    {
        private readonly Random random;
        private readonly object randomLock = new object();

        private readonly ISession localSession;
        private readonly ILogger log;

        public MessageService(ISession localSession, ILogger log)
        {
            this.localSession = localSession;
            this.log = log;
            this.random = new Random();
        }

        public string Done(object context)
        {
            return this.RetrieveMessage(Messages.Done, context, null);
        }

        public string NotEnoughParameters(object context, string command, int expected, int actual)
        {
            var arguments = new[]
                                {
                                    command, expected.ToString(CultureInfo.InvariantCulture), 
                                    actual.ToString(CultureInfo.InvariantCulture)
                                };

            return this.RetrieveMessage(Messages.NotEnoughParameters, context, arguments);
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
        
        public List<string> RetrieveAllMessagesForKey(string messageKey, object context, IEnumerable<string> arguments)
        {
            if (string.IsNullOrEmpty(messageKey))
            {
                throw new ArgumentNullException("messageKey");
            }

            string contextPath = context != null ? context.ToString() : string.Empty;

            if (!string.IsNullOrEmpty(contextPath))
            {
                contextPath = string.Format("/{0}", contextPath);

                contextPath = contextPath.Replace("#", string.Empty) // will cause issues
                    .Replace("|", string.Empty) // link syntax
                    .Replace("[", string.Empty) // link syntax
                    .Replace("]", string.Empty) // link syntax
                    .Replace("{", string.Empty) // link syntax
                    .Replace("}", string.Empty) // link syntax
                    .Replace("<", string.Empty) // html issues
                    .Replace(">", string.Empty); // html issues
            }

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

            if (arguments != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                object[] args = arguments.ToArray();
                
                for (int i = 0; i < messages.Count; i++)
                {
                    messages[i] = string.Format(messages[i], args);
                }
            }

            return messages;
        }
        
        public void RefreshResponseRepository()
        {
            lock (this.localSession)
            {
                var all = this.localSession.CreateCriteria<Response>().List<Response>();
                foreach (var model in all)
                {
                    this.localSession.Refresh(model);
                }
            }
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
            Response response;
            lock (this.localSession)
            {
                response = this.localSession.CreateCriteria<Response>()
                    .Add(Restrictions.Eq("Name", Encoding.UTF8.GetBytes(messageKey)))
                    .UniqueResult<Response>();
            }
            
            if (response != null)
            {
                // extract the byte array from the dataset
                string text = Encoding.UTF8.GetString(response.Text);
                return text.Split('\n').ToList();
            }

            return new List<string>();
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