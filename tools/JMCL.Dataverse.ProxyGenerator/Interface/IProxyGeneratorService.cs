using JMCL.Dataverse.ProxyGenerator.Model;

namespace JMCL.Dataverse.ProxyGenerator
{
    public interface IProxyGeneratorService : IMessageProvider
    {
        void BuildProxies(ProxyModel model);
    }
}
