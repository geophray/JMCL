namespace JMCL.Dataverse.FieldEncryption
{
    using JMCL.Dataverse.Sdk;

    /// <summary>
    /// Factory for creating <see cref="IEncryptedFieldService"/> configured for a specific record type.
    /// </summary>
    public interface IEncryptedFieldServiceFactory
    {
        IEncryptedFieldService Create(ICDSPluginExecutionContext executionContext, string recordType, bool disableCache = false);
    }
}
