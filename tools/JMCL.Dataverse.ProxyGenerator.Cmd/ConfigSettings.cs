using JMCL.Dataverse.ProxyGenerator;

namespace JMCL.Dataverse.ProxyGenerator.Cmd
{
    public class ConfigSettings : Settings, IConfigSettings
    {
        public string ConfigurationPath { get; set; }
    }
}
