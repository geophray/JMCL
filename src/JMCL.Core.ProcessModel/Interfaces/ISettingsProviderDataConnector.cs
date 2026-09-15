using System.Collections.Generic;

namespace JMCL.Core
{
    public interface ISettingsProviderDataConnector
    {
        IReadOnlyDictionary<string, string> LoadSettings(IDataService dataService);
    }
}
