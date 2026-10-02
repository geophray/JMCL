using System;
using System.Collections.Generic;
using System.Linq;

namespace JMCL.Core
{
    public class SettingsProviderFactory : ISettingsProviderFactory
    {
        private const string CACHE_KEY = "JMCL.SettingsProviderFactory";
        private const string CACHE_TIMEOUT_SETTING = "JMCL.SettingsCacheTimeOut";
       
        private ISettingsProviderDataConnector DataConnector { get; }

        public SettingsProviderFactory(ISettingsProviderDataConnector dataConnector)
        {
            DataConnector = dataConnector;
        }

        public ISettingsProvider CreateSettingsProvider(IProcessExecutionContext executionContext, bool useCache = true)
        {
            if (useCache && executionContext.Cache != null && executionContext.Cache.Exists(CACHE_KEY))
            {
                var cachedSettings = executionContext.Cache.Get<Dictionary<string, string>>(CACHE_KEY);
                return new SettingsProvider(cachedSettings);
            }

            var settings = DataConnector.LoadSettings(executionContext.DataService);

            var settingsProvider = new SettingsProvider(settings);

            if (useCache)
            {
                var cacheTimeout = settingsProvider.GetValue<TimeSpan?>(CACHE_TIMEOUT_SETTING);

                if (cacheTimeout != null && cacheTimeout.Value.Ticks != 0)
                {
                    executionContext.Cache.Add<Dictionary<string, string>>(CACHE_KEY, settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), cacheTimeout.Value);
                }
            }

            return settingsProvider;
        }

    }
}
