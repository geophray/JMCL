using System.Collections.Generic;

namespace JMCL.Dataverse.ProxyGenerator.Cmd
{
    interface IProxySettingsService
    {
        IEnumerable<IConfigSettings> LoadSettings(string path);
    }
}
