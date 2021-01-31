namespace Helpmebot.CoreServices.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Castle.Core;
    using Castle.Core.Logging;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Persistence;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;

    public abstract class CommandParserProviderServiceBase<T> : IStartable
        where T : EntityBase, ICommandParserEntity
    {
        private readonly ICommandParser commandParser;
        private readonly HashSet<CommandParserEntity> registeredCommands;
        
        protected CommandParserProviderServiceBase(ICommandParser commandParser, ILogger logger)
        {
            this.commandParser = commandParser;
            this.Logger = logger;
            this.registeredCommands = new HashSet<CommandParserEntity>();
        }
        
        protected ILogger Logger { get; private set; }
        
        protected abstract IList<T> ItemsToRegister();
        protected abstract Type CommandImplementation();
        
        public void Start()
        {
            var itemsToRegister = this.ItemsToRegister();

            this.Logger.InfoFormat(
                "Populating command parser with {0} items",
                itemsToRegister.Count);

            foreach (var item in itemsToRegister)
            {
                this.RegisterCommand(item);
            }
            
            lock (this.registeredCommands)
            {
                this.Logger.InfoFormat("Registered {0} items in command parser", this.registeredCommands.Count);
            }
        }

        public void Stop()
        {
            HashSet<CommandParserEntity> set;
            lock (this.registeredCommands)
            {
                set = new HashSet<CommandParserEntity>(this.registeredCommands);
                this.Logger.InfoFormat(
                    "Shutting down service with {0} items",
                    this.registeredCommands.Count);
                
                this.registeredCommands.Clear();
            }

            foreach (var command in set)
            {
                this.UnregisterCommand(command);
            }
        }

        protected void RegisterCommand(ICommandParserEntity item)
        {
            if (!this.UnregisterCommand(item))
            {
                return;
            }

            this.commandParser.RegisterCommand(item.CommandKeyword, this.CommandImplementation(), item.CommandChannel);
            
            lock (this.registeredCommands)
            {
                this.registeredCommands.Add(new CommandParserEntity(item));
            }

            this.Logger.DebugFormat("Registered item {0} in command parser.", item.CommandKeyword);
        }

        protected bool UnregisterCommand(ICommandParserEntity item)
        {
            var existingCommand = this.commandParser.GetRegisteredCommand(item.CommandKeyword, item.CommandChannel);
            if (existingCommand != null)
            {
                if (existingCommand != this.CommandImplementation())
                {
                    this.Logger.WarnFormat(
                        "Could not unregister item {0} with command parser as this command is not the correct type.",
                        item.CommandKeyword);
                    return false;
                }

                this.Logger.DebugFormat("Unregistered item {0} from command parser.", item.CommandKeyword);

                lock (this.registeredCommands)
                {
                    this.registeredCommands.Remove(new CommandParserEntity(item));
                }

                this.commandParser.UnregisterCommand(item.CommandKeyword, item.CommandChannel);
            }

            return true;
        }

        private class CommandParserEntity : ICommandParserEntity
        {
            public CommandParserEntity(ICommandParserEntity fromOther)
            {
                this.CommandKeyword = fromOther.CommandKeyword;
                this.CommandChannel = fromOther.CommandChannel;
            }

            public string CommandKeyword { get; private set; }
            public string CommandChannel { get; private set; }

            protected bool Equals(CommandParserEntity other)
            {
                return string.Equals(this.CommandKeyword, other.CommandKeyword) && string.Equals(this.CommandChannel, other.CommandChannel);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return this.Equals((CommandParserEntity) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((this.CommandKeyword != null ? this.CommandKeyword.GetHashCode() : 0) * 397) ^ (this.CommandChannel != null ? this.CommandChannel.GetHashCode() : 0);
                }
            }
        }
    }
}