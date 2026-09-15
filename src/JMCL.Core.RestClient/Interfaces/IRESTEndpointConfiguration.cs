namespace JMCL.Core.RestClient
{
    using JMCL.Core.Net;

    public interface IRESTEndpointConfiguration
    {
        IAPIEndpoint Endpoint { get; }
        string AccessToken { get; }
    }
}