namespace Helpmebot.WebApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Castle.Core.Internal;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.Brain.Services.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.CoreServices.Startup;
    using Helpmebot.Model;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using FlagGroup = Helpmebot.WebApi.TransportModels.FlagGroup;
    using InterwikiPrefix = Helpmebot.WebApi.TransportModels.InterwikiPrefix;
    using Response = Helpmebot.WebApi.TransportModels.Response;

    public class ApiService : IApiService
    {
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly ICommandParser commandParser;
        private readonly BotConfiguration botConfiguration;
        private readonly IIrcConfiguration ircConfiguration;
        private readonly ILoginTokenService loginTokenService;
        private readonly ISessionFactory sessionFactory;
        private readonly IResponseManager responseManager;

        public ApiService(
            ILogger logger,
            IIrcClient client,
            ICommandParser commandParser,
            BotConfiguration botConfiguration,
            IIrcConfiguration ircConfiguration,
            ILoginTokenService loginTokenService,
            ISessionFactory sessionFactory,
            IResponseManager responseManager)
        {
            this.logger = logger;
            this.client = client;
            this.commandParser = commandParser;
            this.botConfiguration = botConfiguration;
            this.ircConfiguration = ircConfiguration;
            this.loginTokenService = loginTokenService;
            this.sessionFactory = sessionFactory;
            this.responseManager = responseManager;
        }

        public IKeywordService BrainKeywordService { get; set; }
        
        public BotStatus GetBotStatus()
        {
            var botVersion = Assembly.GetAssembly(Type.GetType("Helpmebot.Launch, Helpmebot")).GetName().Version;
            
            return new BotStatus
            {
                ChannelCount = this.client.Channels.Count,
                Commands = this.commandParser.GetCommandRegistrations().Keys.ToList(),
                Nickname = this.client.Nickname,
                Trigger = this.botConfiguration.CommandTrigger,
                IrcServer = this.ircConfiguration.Hostname,
                IrcServerPort = this.ircConfiguration.Port,
                PingTime = this.client.Latency,
                StartupTime = Launcher.StartupTime,
                TotalMessages = this.client.PrivmsgReceived,
                VisibleUserCount = this.client.UserCache.Count,
                BotVersion = $"{botVersion.Major}.{botVersion.Minor}.{botVersion.Build}"
            };
        }

        public string GetLoginToken()
        {
            return this.loginTokenService.GetLoginToken();
        }

        public TokenResponse GetAuthToken(string loginToken)
        {
            return this.loginTokenService.GetAuthToken(loginToken);
        }

        public List<BrainItem> GetBrainItems()
        {
            if (this.BrainKeywordService == null)
            {
                this.logger.Warn("GetBrainItems called but BrainKeywordService is null.");
                throw new Exception("Missing API dependency");
            }

            return this.BrainKeywordService.GetAll()
                .Select(x => new BrainItem { IsAction = x.Action, Keyword = x.Name, Response = x.Response })
                .ToList();
        }

        public void InvalidateToken(string authTokenHandle)
        {
            this.loginTokenService.InvalidateToken(authTokenHandle);
        }

        public List<CommandInfo> GetRegisteredCommands()
        {
            var registrationInfo = this.commandParser.GetCommandRegistrations();
            
            // collect all the types from the command registrations
            var types = new HashSet<Type>();
            foreach (var registrationInfoValue in registrationInfo.Values)
            {
                foreach (var reg in registrationInfoValue.Keys)
                {
                    types.Add(reg.Type);
                }
            }
            
            var commandList = new List<CommandInfo>();
            foreach (var commandClass in types)
            {
                var commandInfo = new CommandInfo();
                
                var undocumentedAttribute = commandClass.GetCustomAttributes(typeof(UndocumentedAttribute), false);
                var commandInvocationAttribute = commandClass.GetCustomAttributes(typeof(CommandInvocationAttribute), false);

                if (undocumentedAttribute.Length > 0 || commandInvocationAttribute.Length == 0)
                {
                    continue;
                }
                
                // Name and aliases
                commandInfo.CanonicalName = ((CommandInvocationAttribute) commandInvocationAttribute.First()).CommandName;
                commandInfo.Aliases = commandInvocationAttribute.Select(x => ((CommandInvocationAttribute) x).CommandName)
                    .Where(x => x != commandInfo.CanonicalName )
                    .ToList();
                
                // Help summary
                commandInfo.HelpSummary = commandClass.GetCustomAttributes(typeof(HelpSummaryAttribute), false)
                    .Cast<HelpSummaryAttribute>()
                    .FirstOrDefault()?.Description;
                
                // Help category
                commandInfo.HelpCategory = commandClass.GetCustomAttributes(typeof(HelpCategoryAttribute), true)
                    .Select(x => (HelpCategoryAttribute) x)
                    .FirstOrDefault()?.Category ?? "Default";

                // Main command flags
                commandInfo.Flags = commandClass.GetCustomAttributes(typeof(CommandFlagAttribute), false)
                    .Cast<CommandFlagAttribute>()
                    .Select(x => new CommandInfo.CommandFlag { Flag = x.Flag, GlobalOnly = x.GlobalOnly })
                    .ToList();

                commandInfo.Type = commandClass.FullName;

                var methodInfos = commandClass.GetMethods(
                    BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);

                if (File.Exists($"Documentation/{commandClass.FullName}.md"))
                {
                    commandInfo.ExtendedHelp = File.ReadAllText($"Documentation/{commandClass.FullName}.md");
                }
                
                foreach (var info in methodInfos)
                {
                    var subcommandInfo = new CommandInfo.SubcommandInfo();
                    
                    if (info.IsAbstract || info.IsConstructor || info.IsPrivate)
                    {
                        continue;
                    }

                    var undocumented = info.GetCustomAttributes(typeof(UndocumentedAttribute), false);
                    if (undocumented.Length > 0)
                    {
                        continue;
                    }
                    
                    var invocations = info.GetCustomAttributes(typeof(SubcommandInvocationAttribute), false)
                        .Cast<SubcommandInvocationAttribute>()
                        .ToList();
                    if (invocations.Count == 0 && info.Name != "Execute")
                    {
                        continue;
                    }
                    
                    var helpAttr = info.GetAttribute<HelpAttribute>();
                    if (helpAttr == null)
                    {
                        continue;
                    }
                    
                    subcommandInfo.CanonicalName = commandInfo.CanonicalName;
                    subcommandInfo.Aliases = new List<string>();
                    
                    if (info.Name != "Execute")
                    {
                        subcommandInfo.CanonicalName += " " + invocations.FirstOrDefault()?.CommandName;
                        
                        foreach (var invoke in invocations.Skip(1))
                        {
                            subcommandInfo.Aliases.Add(commandInfo.CanonicalName + " " + invoke.CommandName);
                        }
                    }
                    
                    subcommandInfo.Syntax = helpAttr.HelpMessage.Syntax.ToList();
                    subcommandInfo.HelpText = helpAttr.HelpMessage.Text.ToList();

                    subcommandInfo.Flags = info.GetCustomAttributes(typeof(CommandFlagAttribute), false)
                        .Cast<CommandFlagAttribute>()
                        .Select(x => new CommandInfo.CommandFlag { Flag = x.Flag, GlobalOnly = x.GlobalOnly })
                        .ToList();
                    
                    commandInfo.Subcommands.Add(subcommandInfo);
                }
                
                commandList.Add(commandInfo);
            }

            return commandList;
        }

        public Dictionary<string, Tuple<string, string>> GetFlagHelp()
        {
            var data = new Dictionary<string, Tuple<string, string>>();
            foreach (var fieldInfo in typeof(Flags).GetFields().Where(x => x.IsLiteral && x.FieldType == typeof(string)))
            {
                var flagHelpAttr = fieldInfo.GetCustomAttributes(typeof(FlagHelpAttribute), false).Cast<FlagHelpAttribute>().FirstOrDefault();
                var rawConstantValue = fieldInfo.GetRawConstantValue();
                if (flagHelpAttr == null)
                {
                    continue;
                }
            
                data.Add((string)rawConstantValue, new Tuple<string,string>(flagHelpAttr.QuickHelpText, flagHelpAttr.DetailedHelp));
            }

            return data;
        }

        public List<FlagGroup> GetFlagGroups()
        {
            var session = this.sessionFactory.OpenSession();
            var flagGroups = session.QueryOver<Model.FlagGroup>().List().Select(x => new FlagGroup(x)).ToList();
            
            session.Close();
            return flagGroups;
        }

        public AccessControlList GetAccessControlList()
        {
            var session = this.sessionFactory.OpenSession();
            var userAclDict = session.QueryOver<User>()
                .List()
                .Select(
                    x => new UserAccessControlEntry
                    {
                        AccountName = x.Account, IrcMask = x.Mask,
                        FlagGroups = x.AppliedFlagGroups.Select(fg => fg.Name).ToList()
                    })
                .ToList();
            session.Close(); 
            
            return new AccessControlList { Users = userAclDict };
        }

        public List<InterwikiPrefix> GetInterwikiList()
        {
            var session = this.sessionFactory.OpenSession();
            var interwikiList = session.QueryOver<Model.InterwikiPrefix>()
                .List()
                .Select(x => new InterwikiPrefix(x))
                .ToList();
            session.Close();
            
            return interwikiList;
        }

        public List<string> GetMessageKeys()
        {
            return this.responseManager.GetAllKeys();
        }

        public Response GetMessageDefaultResponses(ResponseKey key)
        {
            var (responses, repository) = this.responseManager.Get(key.MessageKey, key.ContextType, key.Context);

            return new Response { Key = key, Repository = repository, Responses = responses };
        }

        public List<Response> GetMessageResponses()
        {
            var allKeys = this.GetMessageKeys();
            var list = new List<Response>();
            
            foreach (var key in allKeys)
            {
                var objectKey = new ResponseKey { MessageKey = key };
                list.Add(this.GetMessageDefaultResponses(objectKey));
            }

            return list;
        }
    }
}