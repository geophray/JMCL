using System.Collections.Generic;

namespace JMCL.Dataverse.ProxyGenerator.Cmd
{

    public interface ISpklSettingsService
    {
        IEnumerable<IConfigSettings> LoadSettings(string path);
    }
}
