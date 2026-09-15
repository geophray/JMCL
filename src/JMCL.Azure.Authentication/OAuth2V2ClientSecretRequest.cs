using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using JMCL.Core.Net;
using JMCL.Core.Serialization;
using JMCL.Core.RestClient;

namespace JMCL.Azure.Authentication
{
    public class OAuth2V2ClientSecretRequest : SerializedRESTRequest<AuthToken>
    {
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string Scope;

        public OAuth2V2ClientSecretRequest(IJSONContractSerializer serializer, string tenantId, string clientId, string clientSecret, string scope)
                : base(serializer, new APIEndpoint($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"))
        {
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.Scope = scope;
        }

        public override AuthToken Execute(IWebRequest webRequest)
        {
            var data = generateFormData();
            SetRequestHeaders(webRequest);
            var webResponse = webRequest.Post(data, "application/x-www-form-urlencoded");

            return Serializer.Deserialize<AuthToken>(webResponse.Content);
        }

        private byte[] generateFormData()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("scope", Scope),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var stream = new MemoryStream();
            formContent.CopyToAsync(stream).Wait();
            return stream.ToArray();
        }
    }
}
