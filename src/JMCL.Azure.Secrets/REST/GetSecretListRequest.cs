using JMCL.Core.Net;
using JMCL.Core.Serialization;

namespace JMCL.Azure.Secrets
{
    public class GetSecretListRequest : AzureRestRequest<SecretList>
    {
        public GetSecretListRequest(IJSONContractSerializer serializer, AuthToken token, string vaultName)
           : base(serializer, token, new APIEndpoint(string.Format("https://{0}.vault.azure.net/secrets?api-version=7.0", vaultName)))
        {
        }
    }
}
