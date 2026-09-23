using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;


namespace JMCL.Dataverse.ProxyGenerator
{ 
    using JMCL.Dataverse.Sdk.Metadata;
    using JMCL.Core;

    public class CDSMetadataService : MessagingBase, ICDSMetadataService
    {
        static class CacheKey
        {
            public static string SkdMessageNames = "SdkMessageNames";
            public static string EntityMetadata = "EntityMetadata";
        }

        protected ICache cache;
        protected readonly ISettings settings;

        private ISdkMessageMetadataService _sdkMessageMetadataService;
        private ISdkMessageMetadataService SdkMessageMetadataService
        {
            get
            {
                if (_sdkMessageMetadataService is null)
                {
                    _sdkMessageMetadataService = new SdkMessageMetadataService(settings.TargetEndPoint);
                }

                return _sdkMessageMetadataService;
            }
        }
        
        public CDSMetadataService(ISettings settings, ICache cache) : base()
        {
            this.settings = settings ?? throw new ArgumentNullException("settings");
            this.cache = cache;
        }

        public IEnumerable<EntityMetadata> GetEntityMetadata(IOrganizationService orgService)
        {
            if(settings.EntitiesToInclude is null || settings.EntitiesToInclude.Count() == 0 || string.IsNullOrEmpty(settings.EntitiesToInclude.FirstOrDefault()))
            {
                RaiseMessage("No entities requested...");
                return new List<EntityMetadata>();
            }

            var metadata = GetAllEntityMetadata(orgService);
            metadata = FilterEntityMetadata(metadata);

            RaiseMessage(string.Format("Processed metadata for {0} entities", metadata.Count()));

            return metadata;
        }

        public IEnumerable<SdkMessageMetadata> GetMessageMetadata(IOrganizationService orgService)
        {    
            if(settings.ActionsToInclude is null || settings.ActionsToInclude.Count() == 0 || string.IsNullOrEmpty(settings.ActionsToInclude.FirstOrDefault()))
            {
                RaiseMessage("No Sdk Messages requested...");
                return new List<SdkMessageMetadata>();
            }

            var names = GetAllSdkMessageNames(orgService, settings.ActionsToInclude);
            
            names = FilterSdkMessageNames(names);

            var messageMetadata = GetMessageMetadataFromCache(names);
            
            // filter to names that are not in the cached metadata
            names = names.Where(n => !messageMetadata.Where(c => c.Name == n).Any());

            if (names.Count() > 0)
            {
                RaiseMessage("Getting Sdk MessageMetadata...");

                var retrievedMetadata = SdkMessageMetadataService.GetSdkMessageMetadata(orgService, names);

                AddMessageMetadataToCache(retrievedMetadata);

                messageMetadata.AddRange(retrievedMetadata);

                RaiseMessage(string.Format("Retrieved metadata for {0} Sdk Messages...", messageMetadata.Count()));
            }

            return messageMetadata;
        }

        private IEnumerable<EntityMetadata> GetAllEntityMetadata(IOrganizationService orgService)
        {
            if (cache.Exists(CacheKey.EntityMetadata))
                return cache.Get<EntityMetadata[]>(CacheKey.EntityMetadata);

            RaiseMessage("Gathering Entity Metadata...");

            OrganizationRequest request = new OrganizationRequest("RetrieveAllEntities");

            request.Parameters["EntityFilters"] = EntityFilters.Relationships | EntityFilters.Attributes | EntityFilters.Entity;
            request.Parameters["RetrieveAsIfPublished"] = true;

            var entityMetadata = orgService.Execute(request).Results["EntityMetadata"] as EntityMetadata[];

            cache.Add(CacheKey.EntityMetadata, entityMetadata, new TimeSpan(0,5,0));

            return entityMetadata;
        }

        private IEnumerable<EntityMetadata> FilterEntityMetadata(IEnumerable<EntityMetadata> entityMetadata)
        {
            RaiseMessage("Filtering Entity Metadata...");

            var entitiesToInclude = new MatchEvaluator(settings.EntitiesToInclude);

            var entitiesToExclude = new MatchEvaluator(settings.EntitiesToExclude);

            var filteredMetadata = entityMetadata
                .Where(r => ShouldIncludeMetadata(r.LogicalName, entitiesToInclude, entitiesToExclude))
                .ToArray();

            return filteredMetadata;
        }

        private IEnumerable<string> GetAllSdkMessageNames(IOrganizationService orgService, IEnumerable<string> actionsToInclude)
        {
            if (cache.Exists(CacheKey.SkdMessageNames))
                return cache.Get<IEnumerable<string>>(CacheKey.SkdMessageNames);


            RaiseMessage("Gathering Sdk Message Names...");

            var names = SdkMessageMetadataService.GetSdkMessageNames(orgService, actionsToInclude);
            cache.Add(CacheKey.SkdMessageNames, names, 300);

            RaiseMessage(string.Format("Added {0} Sdk Message Names to cache...", names.Count()));

            return names;
        }

        private IEnumerable<string> FilterSdkMessageNames(IEnumerable<string> messageNames)
        {
            RaiseMessage("Filtering Sdk Message Names...");

            var actionsToInclude = new MatchEvaluator(settings.ActionsToInclude);

            var actionsToExclude = new MatchEvaluator(settings.ActionsToExclude);

            var filteredNames = messageNames
                .Where(r => ShouldIncludeMetadata(r, actionsToInclude, actionsToExclude))
                .ToArray();

            return filteredNames;
        }

        private void AddMessageMetadataToCache(IEnumerable<SdkMessageMetadata> metadata)
        {
            foreach(var m in metadata)
            {
                var key = string.Format("{0}.{1}", settings.TargetEndPoint, m.Name);
                cache.Add(key, m, new TimeSpan(0, 5, 0));
            }
        }

        private List<SdkMessageMetadata> GetMessageMetadataFromCache(IEnumerable<string> messageNames)
        {
            var items = new List<SdkMessageMetadata>();

            foreach(var name in messageNames)
            {
                var key = string.Format("{0}.{1}", settings.TargetEndPoint, name);
                if (cache.Exists(key))
                {
                    items.Add(cache.Get<SdkMessageMetadata>(key));
                }
            }

            return items;
        }
        

        private bool ShouldIncludeMetadata(string logicalName, IMatchEvaluator entitiesToInclude, IMatchEvaluator entitiesToExclude)
        {
            var includeWeight = entitiesToInclude.ScoreMatch(logicalName);
            var excludeWeight = entitiesToExclude.ScoreMatch(logicalName);

            return (includeWeight > excludeWeight);
        }

    }
}
