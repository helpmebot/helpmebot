namespace Helpmebot.Commands.Commands.BotManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Configuration)]
    [CommandInvocation("message")]
    [CommandInvocation("response")]
    [HelpSummary("Manages the bot's responses")]
    public class MessageCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IResponseManager responseManager;

        public MessageCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
            IResponseManager responseManager) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
            this.responseManager = responseManager;
        }

        [Help("<global|local> <key> <alternate> <line> <value...>", "Changes a message line for the specified alternate in the database to the provided value, creating new if necessary")]
        [SubcommandInvocation("set")]
        [RequiredArguments(5)]
        protected IEnumerable<CommandResponse> Set()
        {
            var key = this.Arguments[1];
            int.TryParse(this.Arguments[2], out var alternate);
            var alternateIdx = alternate - 1;
            int.TryParse(this.Arguments[3], out var line);
            var lineIdx = line - 1;
            var value = string.Join(" ", this.Arguments.Skip(4));
            var context = this.Arguments[0] == "local" ? this.CommandSource : null;
            var contextType = this.Arguments[0] == "local" ? Context.Channel : null;

            var (responses, _) = this.responseManager.Get(key, contextType?.ContextType, context);
            if (responses == null)
            {
                responses = new List<List<string>>();
            }

            if (alternateIdx >= responses.Count)
            {
                alternateIdx = responses.Count;
                responses.Add(new List<string>());
            }

            if (lineIdx >= responses[alternateIdx].Count)
            {
                lineIdx = responses[alternateIdx].Count;
                responses[alternateIdx].Add(string.Empty);
            }

            responses[alternateIdx][lineIdx] = value;

            this.responseManager.Set(key, contextType?.ContextType, context, responses);

            return this.responder.Respond("common.done", this.CommandSource);
        }
        
        [Help("<global|local> <key> <alternate> <line>", "Removes the specified alternate message or message line from the database, removing the entire alternate or message from the database if is is the last one.")]
        [SubcommandInvocation("remove")]
        [SubcommandInvocation("del")]
        [RequiredArguments(4)]
        protected IEnumerable<CommandResponse> Remove()
        {
            var key = this.Arguments[1];
            int.TryParse(this.Arguments[2], out var alternate);
            var alternateIdx = alternate - 1;
            int.TryParse(this.Arguments[3], out var line);
            var lineIdx = line - 1;
            var context = this.Arguments[0] == "local" ? this.CommandSource : null;
            var contextType = this.Arguments[0] == "local" ? Context.Channel : null;

            var (responses, _) = this.responseManager.Get(key, contextType?.ContextType, context);
            if (responses == null)
            {
                responses = new List<List<string>>();
            }

            if (alternateIdx >= responses.Count)
            {
                throw new CommandErrorException("Message alternative does not exist.");
            }

            if (lineIdx >= responses[alternateIdx].Count)
            {
                throw new CommandErrorException("Message line does not exist.");
            }

            responses[alternateIdx].RemoveAt(lineIdx);
            if (responses[alternateIdx].Count == 0)
            {
                responses.RemoveAt(alternateIdx);
            }

            if (responses.Count > 0)
            {
                this.responseManager.Set(key, contextType?.ContextType, context, responses);
            }
            else
            {
                this.responseManager.Remove(key, contextType?.ContextType, context);
            }

            return this.responder.Respond("common.done", this.CommandSource);
        }
        
    }
}