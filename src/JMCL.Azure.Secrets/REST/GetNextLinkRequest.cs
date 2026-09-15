
using System;
using System.Collections.Generic;
using System.Text;

namespace JMCL.Azure.Secrets
{
    using JMCL.Core.Net;
    using JMCL.Core.Serialization;

    public class GetNextLinkRequest : AzureRestRequest<SecretList>
    {
        public GetNextLinkRequest(IJSONContractSerializer serializer, AuthToken token, string nextLink) 
            : base(serializer, token, new APIEndpoint(nextLink))
        {
        }
    }
}
