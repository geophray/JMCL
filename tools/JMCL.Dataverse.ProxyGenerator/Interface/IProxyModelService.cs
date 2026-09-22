using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator
{
    using JMCL.Dataverse.Sdk.Metadata;
    using Model;

    public interface IProxyModelService : IMessageProvider
    {
        ProxyModel BuildModel(IEnumerable<EntityMetadata> entityMetadata, IEnumerable<SdkMessageMetadata> messageMetadata);
    }
}
