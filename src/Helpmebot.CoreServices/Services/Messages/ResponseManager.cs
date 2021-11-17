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

        public IEnumerable<CommandResponse> Respond(string messageKey, string channel, object argument)
        {
            return this.Respond(messageKey, channel, new[] { argument }, contextType: Context.Channel);
        }

        public IEnumerable<CommandResponse> Respond(
            string messageKey,
            string context,
            object[] arguments,
            Context contextType = null,
            CommandResponseDestination destination = CommandResponseDestination.Default,
            CommandResponseType type = CommandResponseType.Message,
            bool ignoreRedirection = false,
            IEnumerable<string> redirectionTarget = null)
        {
            List<string> redirectionTargetList = null;
            if (redirectionTarget != null)
            {
                redirectionTargetList = redirectionTarget.ToList();
            }

            return this.GetMessagePartAlternates(messageKey, context, arguments, contextType, false).Select(
                parsedString =>
                {
                    if (parsedString.StartsWith("#ACTION"))
                    {
                        return new CommandResponse
                        {
                            Message = parsedString.Substring("#ACTION ".Length),
                            ClientToClientProtocol = "ACTION",
                            IgnoreRedirection = true,
                            Destination = destination,
                            Type = type,
                            RedirectionTarget = redirectionTargetList
                        };
                    }

                    return new CommandResponse
                    {
                        Message = parsedString,
                        IgnoreRedirection = ignoreRedirection,
                        Destination = destination,
                        Type = type,
                        RedirectionTarget = redirectionTargetList
                    };
                });
        }
        
        public string GetMessagePart(string messageKey, string context, object argument)
        {
            return this.GetMessagePart(messageKey, context, new[] { argument });
        }

        public string GetMessagePart(string messageKey, string context, object[] arguments = null, Context contextType = null)
        {
            return this.GetMessagePartAlternates(messageKey, context, arguments, contextType, true).First();
        }
        
        private IEnumerable<string> GetMessagePartAlternates(string messageKey, 
            string context, 
            object[] arguments, 
            Context contextType,
            bool singleResult)
        {
            int RandomNumber(int count)
            {
                int i;
                lock (this.random)
                {
                    i = this.random.Next(0, count);
                }

                return i;
            }            
            
            if (context != null && contextType == null)
            {
                contextType = Context.Channel;
            }
            
            this.logger.DebugFormat("Response: {0} / ({2}) {1}", messageKey, context, contextType);

            var messageSets = this.FindMessage(messageKey, contextType.ContextType, context);
            
            if (messageSets == null || messageSets.Count == 0)
            {
                return Array.Empty<string>();
            }
            
            List<string> chosenMessageSet;
            if (singleResult)
            {
                // first message from the first set.
                chosenMessageSet = new List<string> { messageSets.First().First() };
            }
            else
            {
                chosenMessageSet = messageSets[RandomNumber(messageSets.Count)];
            }
            
            var sendableMessages = new List<string>(chosenMessageSet.Count);
            
            foreach (var message in chosenMessageSet)
            {
                if (arguments != null)
                {
                    sendableMessages.Add(string.Format(message, arguments));
                }
                else
                {
                    sendableMessages.Add(message);
                }
            }

            return sendableMessages;
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