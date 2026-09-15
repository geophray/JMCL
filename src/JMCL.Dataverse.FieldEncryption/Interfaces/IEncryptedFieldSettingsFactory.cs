namespace JMCL.Dataverse.FieldEncryption
{
    using JMCL.Core;

    /// <summary>
    /// Factory to create <see cref="IEncryptedFieldSettings"/>.
    /// </summary>
    public interface IEncryptedFieldSettingsFactory
    {
        IEncryptedFieldSettings Create(IProcessExecutionContext executionContext);       
    }
}
