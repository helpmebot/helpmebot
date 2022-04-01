namespace Helpmebot.CoreServices.Services
{
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public class InterwikiService : IInterwikiService
    {
        private readonly ISession database;
        private readonly ILogger logger;
        
        public InterwikiService(ISession database, ILogger logger)
        {
            this.database = database;
            this.logger = logger;
        }

        public (int upToDate, int created, int updated, int deleted) Import(IMediaWikiApi mediaWikiApi)
        {
            lock (this)
            {
                this.logger.Debug("Fetching existing prefixes");
                var allPrefixes = this.database.QueryOver<InterwikiPrefix>()
                    .List().ToDictionary(x => x.Prefix);

                this.logger.Debug("Marking existing as absent");
                foreach (var prefix in allPrefixes)
                {
                    prefix.Value.AbsentFromLastImport = true;
                    prefix.Value.CreatedSinceLast = false;
                }

                this.logger.Debug("Downloading latest map");

                var prefixesToImport = mediaWikiApi.GetInterwikiPrefixes().ToList();

                int deletedIw = 0, createdIw = 0, updatedIw = 0, upToDateIw = 0;

                this.logger.Debug("Iterating import");
                int i = 0;
                foreach (var prefix in prefixesToImport)
                {
                    this.logger.DebugFormat(
                        "  processed {0} of {1} - {2}%",
                        i,
                        prefixesToImport.Count,
                        i / prefixesToImport.Count * 100);
                    i++;

                    if (allPrefixes.ContainsKey(prefix.Prefix))
                    {
                        allPrefixes[prefix.Prefix].AbsentFromLastImport = false;

                        // present, are we up-to-date?
                        if (allPrefixes[prefix.Prefix].Url == prefix.Url)
                        {
                            // yes
                            upToDateIw++;
                            continue;
                        }

                        var existingImport = this.database.QueryOver<InterwikiPrefix>()
                            .Where(x => x.ImportedAs == prefix.Prefix)
                            .SingleOrDefault();
                        if (existingImport == null)
                        {
                            existingImport = new InterwikiPrefix();
                        }

                        existingImport.Prefix = prefix.Prefix + "-import";
                        existingImport.ImportedAs = prefix.Prefix;
                        existingImport.Url = prefix.Url;
                        existingImport.AbsentFromLastImport = false;
                        this.database.SaveOrUpdate(existingImport);
                        updatedIw++;
                    }
                    else
                    {
                        this.database.Save(
                            new InterwikiPrefix
                            {
                                Prefix = prefix.Prefix, Url = prefix.Url,
                                AbsentFromLastImport = false, CreatedSinceLast = true
                            });
                        createdIw++;
                    }
                }

                this.logger.Debug("Saving absent prefixes");
                foreach (var prefix in allPrefixes.Where(x => x.Value.AbsentFromLastImport))
                {
                    deletedIw++;
                    this.database.Update(prefix.Value);
                }

                this.logger.Debug("Flushing session");
                this.database.Flush();

                return (upToDateIw, createdIw, updatedIw, deletedIw);
            }
        }

        public void ForgetMissing()
        {
            lock (this)
            {
                var absentMarked = this.database.QueryOver<InterwikiPrefix>()
                    .Where(x => x.AbsentFromLastImport || x.CreatedSinceLast)
                    .List();

                foreach (var prefix in absentMarked)
                {
                    prefix.AbsentFromLastImport = false;
                    prefix.CreatedSinceLast = false;
                    this.database.Update(prefix);
                }
                this.database.Flush();
            }
        }

        public bool Accept(string prefix)
        {
            lock (this)
            {
                var imported = this.database.QueryOver<InterwikiPrefix>()
                    .Where(x => x.ImportedAs == prefix)
                    .SingleOrDefault();

                if (imported == null)
                {
                    return false;
                }
            
                var existing = this.database.QueryOver<InterwikiPrefix>()
                    .Where(x => x.Prefix == imported.ImportedAs)
                    .SingleOrDefault();
                
                if (existing == null)
                {
                    imported.Prefix = imported.ImportedAs;
                    imported.ImportedAs = null;
                    this.database.Update(imported);
                }
                else
                {
                    existing.Url = imported.Url;
                    this.database.Update(existing);
                    this.database.Delete(imported);
                }
            
                this.database.Flush();
            }
            
            return true;
        }

        public bool Reject(string prefix)
        {
            lock (this)
            {
                var imported = this.database.QueryOver<InterwikiPrefix>()
                    .Where(x => x.ImportedAs == prefix)
                    .SingleOrDefault();

                if (imported == null)
                {
                    return false;
                }
            
                this.database.Delete(imported);
                this.database.Flush();
                return true;
            }
        }
        
        public void AddOrUpdate(string prefix, string url, out bool updated)
        {
            lock (this)
            {
                var existing = this.database.QueryOver<InterwikiPrefix>()
                    .Where(x => x.Prefix == prefix)
                    .SingleOrDefault();

                updated = true;
                if (existing == null)
                {
                    updated = false;
                    existing = new InterwikiPrefix{Prefix = prefix};
                }

                existing.Url = url;

                this.database.SaveOrUpdate(existing);
                this.database.Flush();
            }
        }

        public bool Delete(string prefix)
        {
            lock (this)
            {
                var existing = this.database.QueryOver<InterwikiPrefix>()
                    .Where(x => x.Prefix == prefix)
                    .SingleOrDefault();

                if (existing == null)
                {
                    return false;
                }

                this.database.Delete(existing);
                this.database.Flush();
            }

            return true;
        }
    }
}