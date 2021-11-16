namespace Helpmebot.CoreServices.Services.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;

    /// <summary>
    /// Class which encapsulates all aspects of managed responses
    /// </summary>
    public class ResponseManager : IResponder, IResponseManager
    {
        
        private readonly ILogger logger;
        private readonly List<IMessageRepository> messageRepositories;
        
        private readonly Random random = new Random();

        public ResponseManager(
            ILogger logger,
            FileMessageRepository fileMessageRepository,
            DatabaseMessageRepository databaseMessageRepository,
            WikiMessageRepository wikiMessageRepository)
        {
            this.logger = logger;
            this.messageRepositories = new List<IMessageRepository>
            {
                databaseMessageRepository,
                wikiMessageRepository,
                fileMessageRepository
            };
        }

        public IEnumerable<CommandResponse> Respond(string messageKey, string contextType, string context, params string[] arguments)
        {
            this.logger.DebugFormat("Response: {0} / ({2}) {1}", messageKey, context, contextType);

            int RandomNumber(List<List<string>> list)
            {
                int i;
                lock (this.random)
                {
                    i = this.random.Next(0, list.Count);
                }

                return i;
            }

            var messageSets = this.FindMessage(messageKey, contextType, context);

            if (messageSets == null || messageSets.Count == 0)
            {
                yield break;
            }

            var chosenMessageSet = messageSets[RandomNumber(messageSets)];
            foreach (var message in chosenMessageSet)
            {
                // ReSharper disable once CoVariantArrayConversion
                var parsedString = string.Format(message, arguments);
                
                if (parsedString.StartsWith("#ACTION"))
                {
                    parsedString = parsedString.Substring(8);
                    yield return new CommandResponse { Message = parsedString, ClientToClientProtocol = "ACTION"};
                }
                else
                {
                    yield return new CommandResponse { Message = parsedString };
                }
            }
        }
        
        void IResponseManager.Set(string messageKey, string contextType, string context, List<List<string>> messageData)
        {
            this.PerformWrite(contextType, context, repo => repo.Set(messageKey, contextType, context, messageData));
        }
        
        List<List<string>> IResponseManager.Get(string messageKey, string contextType, string context)
        {
            return this.FindMessage(messageKey, contextType, context);
        }
        
        void IResponseManager.Remove(string messageKey, string contextType, string context)
        {
            this.PerformWrite(contextType, context, repo => repo.Remove(messageKey, contextType, context));
        }
        
        private void PerformWrite(string contextType, string context, Action<IMessageRepository> action)
        {
            if (contextType == null && context == null)
            {
                action(this.messageRepositories.FirstOrDefault(x => !x.SupportsContext && x.SupportsWrite));
            }
            else
            {
                action(this.messageRepositories.FirstOrDefault(x => x.SupportsContext && x.SupportsWrite));
            }
        }
        private List<List<string>> FindMessage(string messageKey, string contextType, string context)
        {
            foreach (var repo in this.messageRepositories.Where(x => x.SupportsContext))
            {
                this.logger.TraceFormat("Searching: {0} (context)", repo.GetType());
                
                try
                {
                    var result = repo.Get(messageKey, contextType, context);

                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Warn("Error encountered finding message", ex);
                }
            }
            
            foreach (var repo in this.messageRepositories)
            {
                this.logger.TraceFormat("Searching: {0}", repo.GetType());
                
                try
                {
                    var result = repo.Get(messageKey, null, null);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Warn("Error encountered finding message", ex);
                }
            }

            this.logger.WarnFormat("Not found: {0}", messageKey);
            return null;
        }
    }
}