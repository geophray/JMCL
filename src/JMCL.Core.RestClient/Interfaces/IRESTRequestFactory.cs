namespace JMCL.Core.RestClient
{
    public interface IRESTRequestFactory
    {
        T CreateRequest<T,R>(IRESTEndpointConfiguration configuration) where T : class, IRESTRequest<R> where R : class, IRESTResponse;
    }
}
