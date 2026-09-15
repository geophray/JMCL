namespace JMCL.Azure.Secrets
{
    using JMCL.Core;

    public interface ISecretProviderFactory
    {
        ISecretProvider Create(IProcessExecutionContext executionContext, bool disableCache = false);
    }
}
