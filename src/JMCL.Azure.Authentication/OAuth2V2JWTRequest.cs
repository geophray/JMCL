using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using JMCL.Core.Net;
using JMCL.Core.Serialization;
using JMCL.Core.RestClient;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace JMCL.Azure.Authentication
{
    public class OAuth2V2JWTRequest : SerializedRESTRequest<AuthToken>
    {
        private readonly string TenantId;
        private readonly string ClientId;
        private readonly X509Certificate2 Certificate;
        private readonly string Scope;

        public OAuth2V2JWTRequest(IJSONContractSerializer serializer, string tenantId, string clientId, X509Certificate2 certificate, string scope)
                : base(serializer, new APIEndpoint($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"))
        {
            this.TenantId = tenantId;
            this.ClientId = clientId;
            this.Certificate = certificate;
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
            var assertion = GenerateAssertion();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                new KeyValuePair<string, string>("client_assertion", assertion),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("scope", Scope),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
            });

            var stream = new MemoryStream();
            formContent.CopyToAsync(stream).Wait();
            return stream.ToArray();
        }


        private string GenerateAssertion()
        {            
            var thumbPrintBytes = Certificate.GetCertHash();
            var thumbPrint = Base64UrlEncode(thumbPrintBytes);

            var headerObject = new { alg = "RS256", typ = "JWT", kid = thumbPrint };
            var header = JsonSerializer.Serialize(headerObject);
            var headerBytes = Encoding.UTF8.GetBytes(header);
            var encodedHeader = Base64UrlEncode(headerBytes);

            var issueTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var expirationTime = issueTime + 300; // 5 minutes

            var audience = $"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";

            var payloadObject = new { aud = audience, exp = expirationTime, iss = ClientId, jti = Guid.NewGuid().ToString(), nbf = issueTime, sub = ClientId, iat = issueTime };
            var payload = JsonSerializer.Serialize(payloadObject);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var encodedPayload = Base64UrlEncode(payloadBytes);

            var key = Certificate.GetRSAPrivateKey();
            var signature = ComputeSignature(encodedHeader, encodedPayload, key);

            return $"{encodedHeader}.{encodedPayload}.{signature}";
        }

        static string ComputeSignature(string encodedHeader, string encodedPayload, RSA key)
        {
            string input = $"{encodedHeader}.{encodedPayload}";
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            var hash = key.SignData(inputBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Base64UrlEncode(hash);
        }

        static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}

