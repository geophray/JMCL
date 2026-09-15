using JMCL.Core;

namespace JMCL.Dataverse.FieldEncryption
{
    public interface IEncryptionServiceFactory 
    {
        IEncryptionService Create(IProcessExecutionContext executionContext, bool disableCache = false);
    }
}
