using System;
using Microsoft.Xrm.Sdk;

namespace JMCL.Dataverse.Sdk
{
    using JMCL.Core;

    public class XmlConfigurationResourceFactory : IXmlConfigurationResourceFactory
    {
        private ICache Cache { get; }

        public XmlConfigurationResourceFactory(ICache cache)
        {
            Cache = cache ?? throw new ArgumentNullException("cache is required.");
        }

        public IXmlConfigurationResource CreateConfigurationResources(IOrganizationService orgService)
        {
            return new XmlConfigurationResource(orgService, Cache);
        }
    }
}
