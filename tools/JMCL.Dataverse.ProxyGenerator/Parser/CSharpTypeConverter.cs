namespace JMCL.Dataverse.ProxyGenerator.Parser
{
    using Model;
    public class CSharpTypeCoverter : ITypeConverter
    {
        public TypeModel GetTypeModel(eCdsDataType dataType, string enumTypeName)
        {           
            switch (dataType)
            {
                case eCdsDataType.Uniqueidentifier:
                    return new TypeModel
                    {
                        NativeType = "Guid",
                        ExposedType = "Guid"
                    };

                case eCdsDataType.Picklist:
                case eCdsDataType.State:
                case eCdsDataType.Status:
                    var returnType = enumTypeName ?? "OptionSetValue";
                    return new TypeModel
                    {
                        NativeType = "OptionSetValue",
                        ExposedType = returnType,
                        ConversionType = returnType != "OptionSetValue" ? eConversionType.OptionSetEnum : eConversionType.None
                    };

                case eCdsDataType.MultiSelectPicklist:
                    
                    return new TypeModel
                    {
                        NativeType = "OptionSetValueCollection",
                        ExposedType = "OptionSetValueCollection",
                        ConversionType =  eConversionType.None
                    };

                case eCdsDataType.BigInt:
                case eCdsDataType.Integer:
                    return new TypeModel
                    {
                        NativeType = "int",
                        ExposedType = "int?"
                    };

                case eCdsDataType.Boolean:
                    return new TypeModel
                    {
                        NativeType = "bool",
                        ExposedType = "bool?"
                    };

                case eCdsDataType.DateTime:
                    return new TypeModel
                    {
                        NativeType = "DateTime",
                        ExposedType = "DateTime?"
                    };

                case eCdsDataType.Decimal:
                    return new TypeModel
                    {
                        NativeType = "decimal",
                        ExposedType = "decimal?"
                    };

                case eCdsDataType.Money:
                    return new TypeModel
                    {
                        NativeType = "Money",
                        ExposedType = "decimal?",
                        ConversionType = eConversionType.MoneyValue
                    };

                case eCdsDataType.Double:
                    return new TypeModel
                    {
                        NativeType = "double",
                        ExposedType = "double?"
                    };

                case eCdsDataType.Lookup:
                case eCdsDataType.Owner:
                case eCdsDataType.Customer:
                    return new TypeModel
                    {
                        NativeType = "EntityReference",
                        ExposedType = "EntityReference"
                    };

                case eCdsDataType.Memo:
                case eCdsDataType.Virtual:
                case eCdsDataType.EntityName:
                case eCdsDataType.String:
                    return new TypeModel
                    {
                        NativeType = "string",
                        ExposedType = "string"
                    };

                    
                default:
                    return new TypeModel
                    {
                        NativeType = "object",
                        ExposedType = "object"
                    };
            }
        }
    }
}
