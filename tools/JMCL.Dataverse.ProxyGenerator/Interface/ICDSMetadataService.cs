using System.Collections.Generic;
using JMCL.Dataverse.Sdk.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator
{
    public interface ICDSMetadataService : IMessageProvider
    {
        IEnumerable<EntityMetadata> GetEntityMetadata(IOrganizationService orgService);
        IEnumerable<SdkMessageMetadata> GetMessageMetadata(IOrganizationService orgService);
    }
}
