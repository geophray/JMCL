using JMCL.Dataverse.Sdk.Metadata;
using System.Collections.Generic;

namespace JMCL.Dataverse.ProxyGenerator
{
    public class Settings : ISettings
    {
        public Settings()
        {
            TargetEndPoint = eEndpoint.OrgService;
            TemplateLanguage = eTemplalteLanguage.Csharp;

            // defaults to exclude all entities so only entities that are specifically included will be 
            // represented in proxies.
            EntitiesToExclude = new string[] { "*" };
            ActionsToExclude = new string[] { "*" };

        }

        public string Namespace { get; set; }
        public string TemplateFilePath { get; set; }
        public string OutputPath { get; set; }
        public IEnumerable<string> EntitiesToExclude { get; set; }
        public IEnumerable<string> EntitiesToInclude { get; set; }
        public IEnumerable<string> ActionsToInclude { get; set; }
        public IEnumerable<string> ActionsToExclude { get; set; }

        public eEndpoint TargetEndPoint { get; set; }

        public eTemplalteLanguage TemplateLanguage { get; set; }
    }
}
