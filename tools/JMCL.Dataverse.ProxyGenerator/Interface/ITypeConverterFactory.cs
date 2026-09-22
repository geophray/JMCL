
namespace JMCL.Dataverse.ProxyGenerator
{
    public interface ITypeConverterFactory
    {
        ITypeConverter Create(eTemplalteLanguage language);
    }
}
