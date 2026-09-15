using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using JMCL.Core.Net;
using JMCL.Core.Serialization;
using JMCL.Core.RestClient;

namespace JMCL.Azure.Authentication
{
    [Obsolete("Request is based on V1 end point and may no longer be accepted. Use OAuth2V2ClientSecretRequest or OAuth2V2JWTRequest.")]
    public class GetAuthTokenRequest : SerializedRESTRequest<AuthToken>
    {
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string Resource;

        public GetAuthTokenRequest(IJSONContractSerializer serializer, string tenantId, string clientId, string clientSecret, string resource) 
            : base(serializer, new APIEndpoint("https://login.windows.net/" + tenantId + "/oauth2/token"))
        {                    
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.Resource = resource;
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
                new KeyValuePair<string, string>("resource", Resource),
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
