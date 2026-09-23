using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace JMCL.Dataverse.ProxyGenerator.Cmd.Spkl
{
    public class SpklConfigFile
    {       
        public List<SpklEarlyBoundTypeConfig> earlyboundtypes;
        
        [JsonIgnore]
        public string filePath;

        public virtual SpklEarlyBoundTypeConfig[] GetEarlyBoundConfig(string profile)
        {
            if (earlyboundtypes == null)
                return new SpklEarlyBoundTypeConfig[] { new SpklEarlyBoundTypeConfig(){                    
                    entities = "account,contact",
                    classNamespace = "Proxy"
                } };

            SpklEarlyBoundTypeConfig[] config = null;
            if (profile == "default")
            {
                profile = null;
            }
            if (profile != null)
            {
                config = earlyboundtypes.Where(c => c.profile != null && c.profile.Replace(" ", "").Split(',').Contains(profile)).ToArray();
            }
            else
            {
                // Default profile or empty
                config = earlyboundtypes.Where(c => c.profile == null || c.profile.Replace(" ", "").Split(',').Contains("default") || String.IsNullOrWhiteSpace(c.profile)).ToArray();
            }

            return config;
        }

    }

   
}
