using JMCL.Dataverse.ProxyGenerator;
using JMCL.Dataverse.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace JMCL.Dataverse.ProxyGenerator.Cmd.Spkl
{
    public class SpklEarlyBoundTypeConfig : IConfigSettings
    {
        public string profile;
        public string entities;
        public string actions;
        public string filename;
        public string classNamespace;

        public string templatePath;


        string ISettings.Namespace => classNamespace ?? "Proxy";

        string ISettings.TemplateFilePath => templatePath ?? "proxytemplate.t4";

        string ISettings.OutputPath => Path.GetDirectoryName(filename);

        IEnumerable<string> ISettings.EntitiesToExclude => new string[] { "*" };

        IEnumerable<string> ISettings.EntitiesToInclude => SplitList(entities);

        IEnumerable<string> ISettings.ActionsToInclude => SplitList(actions);

        IEnumerable<string> ISettings.ActionsToExclude => new string[] { "*" };

        eEndpoint ISettings.TargetEndPoint => eEndpoint.OrgService;

        eTemplalteLanguage ISettings.TemplateLanguage => eTemplalteLanguage.Csharp;

        string IConfigSettings.ConfigurationPath { get; set; }

        private IEnumerable<string> SplitList(string value)
        {
            return value?.Split(',').Select(v => v.Trim()) ?? new string[] { };
        }
    }
}
