using Microsoft.Xrm.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator.Parser
{
    using Extensions;
    using Model;

    public class FieldParser
    {
        public static FieldModel Parse(EntityModel parent, AttributeMetadata metadata)
        {            
            var fieldModel = new FieldModel(
                parent,                
                metadata.LogicalName,
                metadata.SchemaName,
                metadata.DisplayName(),
                metadata.PrivatePropertyName(),
                metadata.CdsDataType(),
                metadata.IsPrimaryKey(),
                metadata.IsPrimaryName(),
                metadata.IsLookup(),
                metadata.IsRequired(),
                metadata.IsUpdatable(),
                metadata.IsCreateable(),
                metadata.MinValue(),
                metadata.MaxValue(),
                metadata.MaxLength(),
                metadata.IsAuditable());

            EnumModel enumModel = null;
            if(metadata is PicklistAttributeMetadata)
            {
                enumModel = EnumParser.Parse(fieldModel, metadata as PicklistAttributeMetadata);
            }
            if(metadata is MultiSelectPicklistAttributeMetadata)
            {                        
                enumModel = EnumParser.Parse(fieldModel, metadata as MultiSelectPicklistAttributeMetadata);
            }
            if(metadata is StateAttributeMetadata)
            {
                enumModel = EnumParser.Parse(fieldModel, metadata as StateAttributeMetadata);
            }
            if(metadata is StatusAttributeMetadata)
            {
                enumModel = EnumParser.Parse(fieldModel, metadata as StatusAttributeMetadata);
            }
           

            fieldModel.Enum = enumModel;

            return fieldModel;
        }
    }
}
