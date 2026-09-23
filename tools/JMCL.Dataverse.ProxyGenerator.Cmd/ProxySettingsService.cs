using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JMCL.Dataverse.ProxyGenerator.Cmd
{
    public class ProxySettingsService : IProxySettingsService
    {
        private readonly IDirectoryService DirectoryService;

        public ProxySettingsService(IDirectoryService directoryService)
        {
            this.DirectoryService = directoryService ?? throw new ArgumentNullException("directoryService");
        }

        public IEnumerable<IConfigSettings> LoadSettings(string folder)
        {
            IEnumerable<string> configfilePaths = null;

            // search for the spkl.json configuration file.
            if (folder.EndsWith("proxies.json") && File.Exists(folder))
            {
                configfilePaths = new List<string> { folder };
            }
            else
            {
                configfilePaths = DirectoryService.Search(folder, "proxies.json")
                    .Where(f => f != null && !Regex.IsMatch(f, @"packages\\proxygenerator[0-9|.]*\\tools"));
            }

            var settings = new List<IConfigSettings>();

            foreach (var file in configfilePaths)
            {
                var proxyConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<ProxyConfigFile>(File.ReadAllText(file));
                var setting = proxyConfig as IConfigSettings;

                if (setting != null)
                {
                    setting.ConfigurationPath = Path.GetDirectoryName(file);
                    settings.Add(setting);
                }
            }

            // return null if settings collection is empty
            return settings.Count > 0 ? settings : null;
        }
    }
}
