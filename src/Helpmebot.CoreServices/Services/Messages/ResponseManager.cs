namespace Helpmebot.CoreServices.Services.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            DatabaseMessageRepository databaseMessageRepository)
        {
            this.logger = logger;
            this.messageRepositories = new List<IMessageRepository>
            {
                databaseMessageRepository,
                fileMessageRepository
            };
            
            this.logger.Info("Precaching responses...");
            var sw = Stopwatch.StartNew();
            foreach (var key in this.messageRepositories.SelectMany(x => x.GetAllKeys()).Distinct())
            {
                this.FindMessage(key, null, null);
            }
            sw.Stop();
            this.logger.InfoFormat("Done precaching responses; elapsed {0}ms", sw.ElapsedMilliseconds);

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
                    string ctcp = null;
                    var msgIgnoreRedir = ignoreRedirection;
                    var msgDest = destination;
                    var msg = parsedString;
                    
                    if (msg.StartsWith("#ACTION"))
                    {
                        ctcp = "ACTION";
                        msgIgnoreRedir = true;
                        msg = msg.Substring("#ACTION ".Length);
                    }
                    
                    if (msg.StartsWith("#PRIVMSG"))
                    {
                        msgDest = CommandResponseDestination.PrivateMessage;
                        msg = msg.Substring("#PRIVMSG ".Length);    
                        msgIgnoreRedir = true;
                    }

                    return new CommandResponse
                    {
                        Message = msg,
                        ClientToClientProtocol = ctcp,
                        IgnoreRedirection = msgIgnoreRedir,
                        Destination = msgDest,
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
            return this.GetMessagePartAlternates(messageKey, context, arguments, contextType, true).FirstOrDefault();
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

            (var messageSets, _) = this.FindMessage(messageKey, contextType.ContextType, context);
            
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
        
        (List<List<string>>, string) IResponseManager.Get(string messageKey, string contextType, string context)
        {
            (var messages, var repository) = this.FindMessage(messageKey, contextType, context);
            return (messages, repository.RepositoryType);
        }
        
        void IResponseManager.Remove(string messageKey, string contextType, string context)
        {
            this.PerformWrite(contextType, context, repo => repo.Remove(messageKey, contextType, context));
        }

        List<string> IResponseManager.GetAllKeys()
        {
            return this.messageRepositories.SelectMany(x => x.GetAllKeys()).Distinct().ToList();
        }

        private void PerformWrite(string contextType, string context, Action<IMessageRepository> action)
        {
            if (contextType == null && context == null)
            {
                var repository = this.messageRepositories.FirstOrDefault(x => x.SupportsWrite);
                if (repository == null)
                {
                    throw new Exception("Unable to find writable repository to accept message");
                }

                action(repository);
            }
            else
            {
                var repository = this.messageRepositories.FirstOrDefault(x => x.SupportsContext && x.SupportsWrite);
                if (repository == null)
                {
                    throw new Exception("Unable to find writable repository to accept message");
                }

                action(repository);
            }
        }
        private (List<List<string>>, IMessageRepository) FindMessage(string messageKey, string contextType, string context)
        {
            foreach (var repo in this.messageRepositories.Where(x => x.SupportsContext))
            {
                this.logger.TraceFormat("Searching: {0} (context)", repo.GetType());
                
                try
                {
                    var result = repo.Get(messageKey, contextType, context);

                    if (result != null)
                    {
                        this.logger.TraceFormat("Found in {0} (context)", repo.GetType());
                        return (result, repo);
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
                        this.logger.TraceFormat("Found in {0}", repo.GetType());
                        return (result, repo);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Warn("Error encountered finding message", ex);
                }
            }

            this.logger.ErrorFormat("Message key not found: {0}", messageKey);
            return (null, null);
        }
    }
}