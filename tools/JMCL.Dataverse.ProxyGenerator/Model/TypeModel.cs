using System.Diagnostics.Tracing;

namespace JMCL.Dataverse.ProxyGenerator.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
    public enum eConversionType { None , MoneyValue, OptionSetEnum}

    public class TypeModel
    {
        public eConversionType ConversionType { get; internal set; } = eConversionType.None;
        public string NativeType { get; internal set; }
        public string ExposedType { get; internal set; }
      
    }
}
