using System.Linq;
using Microsoft.Xrm.Sdk.Metadata;


namespace JMCL.Dataverse.ProxyGenerator.Parser
{
    using Extensions;
    using Model;
    public class EnumParser
    {
        public static EnumModel Parse(FieldModel parent, EnumAttributeMetadata metadata)
        {
            if (metadata is null) return null;
            
            var items =
                    metadata?.OptionSet.Options.Select(
                        o => new EnumModelItem
                        {
                            DisplayName = NameHelper.GetProperVariableName(o.Label.UserLocalizedLabel.Label),
                            Value = o.Value ?? 1,                           
                        }
                    ).ToArray();

            return new EnumModel(
                parent, 
                metadata.OptionSet.DisplayName(), 
                metadata?.OptionSet.Name, 
                metadata.OptionSet.IsGlobal ?? false, 
                items);            
        }
    }
}
