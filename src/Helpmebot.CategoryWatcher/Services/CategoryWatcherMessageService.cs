namespace Helpmebot.CategoryWatcher.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using CoreServices.Services.Messages;
    using Interfaces;

    public class CategoryWatcherMessageService : ReadOnlyMessageRepository, ICategoryWatcherMessageService
    {
        private readonly FileMessageRepository fileMessageRepository;
        private readonly IWatcherConfigurationService watcherConfigurationService;

        public CategoryWatcherMessageService(
            FileMessageRepository fileMessageRepository,
            IWatcherConfigurationService watcherConfigurationService)
        {
            this.fileMessageRepository = fileMessageRepository;
            this.watcherConfigurationService = watcherConfigurationService;
        }

        public override bool SupportsContext => false;
        public override string RepositoryType => "catwatcher";

        public override List<List<string>> Get(string key, string contextType, string context)
        {
            if (!key.StartsWith("catwatcher.item."))
            {
                return null;
            }

            var strings = key.Split('.');
            strings[2] = "default";
            var newKey = string.Join(".", strings);

            return this.fileMessageRepository.Get(newKey, contextType, context);
        }

        public override IEnumerable<string> GetAllKeys()
        {
            var defaultKeys = this.fileMessageRepository.GetAllKeys()
                .Where(x => x.StartsWith("catwatcher.item.default."))
                .ToList();

            var allWatchers = this.watcherConfigurationService.GetValidWatcherKeys().ToList();

            var availableKeys = new List<string>(defaultKeys.Count * allWatchers.Count);
            
            foreach (var watcher in allWatchers)
            {
                foreach (var key in defaultKeys)
                {
                    availableKeys.Add(key.Replace("catwatcher.item.default.", $"catwatcher.item.{watcher}."));
                }
            }

            return availableKeys;
        }   
    }
}