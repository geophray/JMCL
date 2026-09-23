using Microsoft.Xrm.Sdk;

namespace JMCL.Dataverse.ProxyGenerator
{
    public interface ICDSMetadataServiceFactory
    {
        ICDSMetadataService Create(ISettings settings);
    }
}
