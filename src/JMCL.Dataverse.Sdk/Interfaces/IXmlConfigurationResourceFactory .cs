using Microsoft.Xrm.Sdk;

namespace JMCL.Dataverse.Sdk
{    
    /// <summary>
    /// Factory for creating <see cref="IXmlConfigurationResource"/> objects.
    /// </summary>
    public interface IXmlConfigurationResourceFactory
    {
        /// <summary>
        /// Factory create method.
        /// </summary>
        /// <param name="orgService"></param>
        /// <returns></returns>
        IXmlConfigurationResource CreateConfigurationResources(IOrganizationService orgService);
    }
}
