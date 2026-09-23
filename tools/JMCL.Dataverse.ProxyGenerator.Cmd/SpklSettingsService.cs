using JMCL.Dataverse.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JMCL.Dataverse.ProxyGenerator.Cmd.Spkl
{
    public class SpklSettingsService : ISpklSettingsService
    {
        private readonly IDirectoryService DirectoryService;

        public SpklSettingsService(IDirectoryService directoryService)
        {
            this.DirectoryService = directoryService ?? throw new ArgumentNullException("directoryService");
        }

        public IEnumerable<IConfigSettings> LoadSettings(string path)
        {
            IEnumerable<string> configfilePaths = null;

            // search for the spkl.json configuration file.
            if (path.EndsWith("spkl.json") && File.Exists(path))
            {
                configfilePaths = new List<string> { path };
            }
            else
            {
                configfilePaths = DirectoryService.Search(path, "spkl.json")
                    .Where(f => f != null && !Regex.IsMatch(f, @"packages\\spkl[0-9|.]*\\tools"));                    
            }

            var settings = new List<IConfigSettings>();

            foreach(var file in configfilePaths)
            {
                var spklConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<SpklConfigFile>(File.ReadAllText(file));
                var setting = spklConfig.earlyboundtypes.FirstOrDefault() as IConfigSettings;

                if(setting != null)
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
