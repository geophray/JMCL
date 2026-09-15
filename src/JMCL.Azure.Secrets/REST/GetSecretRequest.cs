namespace JMCL.Azure.Secrets
{
    using JMCL.Core.Net;
    using JMCL.Core.Serialization;

    public class GetSecretRequest : AzureRestRequest<SecretBundle>
    {
        public GetSecretRequest(IJSONContractSerializer serializer, AuthToken token, string secretId) 
            : base(serializer, token, new APIEndpoint(secretId).AddQuery("api-version","7.0"))
        {            
        }

        
    }
}
