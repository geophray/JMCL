using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Security.Cryptography;

namespace JMCL.Dataverse.ProxyGenerator.Extensions
{
    public static class AttrributeMetadataExtensions
    {
        public static string DisplayName(this AttributeMetadata metadata)
        {
            return NameHelper.GetProperVariableName(metadata.SchemaName);
        }

        public static string PrivatePropertyName(this AttributeMetadata metadata)
        {
            return NameHelper.GetEntityPropertyPrivateName(metadata.SchemaName);
        }

        public static eCdsDataType CdsDataType(this AttributeMetadata metadata)
        {
            var attributeType = metadata.AttributeType ?? throw new System.Exception("Attribute Type is null");

            if (metadata.IsPrimaryKey())
            {
                return eCdsDataType.Uniqueidentifier;
            }

            if (metadata is MultiSelectPicklistAttributeMetadata)
            {
                return eCdsDataType.MultiSelectPicklist;
            }

            return (eCdsDataType)(int)attributeType;
        }

        public static bool IsPrimaryKey(this AttributeMetadata metadata)
        {
            return metadata.IsPrimaryId ?? false;
        }

        public static bool IsPrimaryName(this AttributeMetadata metadata)
        {
            return metadata.IsPrimaryName ?? false;
        }

        public static bool IsRequired(this AttributeMetadata metadata)
        {
            return metadata.RequiredLevel?.Value == AttributeRequiredLevel.ApplicationRequired;
        }

        public static bool IsLookup(this AttributeMetadata metadata)
        {
            return metadata.AttributeType == AttributeTypeCode.Lookup || metadata.AttributeType == AttributeTypeCode.Customer;
        }

        public static bool IsUpdatable(this AttributeMetadata metadata)
        {
            return metadata.IsValidForUpdate ?? false;
        }

        public static bool IsCreateable(this AttributeMetadata metadata)
        {
            return metadata.IsValidForCreate ?? false;
        }       

        public static bool IsAuditable(this AttributeMetadata metadata)
        {
            return metadata.IsAuditEnabled?.Value ?? false;
        }

        public static int? MaxLength(this AttributeMetadata metadata)
        {
            if (metadata is StringAttributeMetadata) { return (metadata as StringAttributeMetadata).MaxLength ?? -1; }
            if (metadata is MemoAttributeMetadata) { return (metadata as MemoAttributeMetadata).MaxLength ?? -1; }

            return null;
        }

        public static decimal? MaxValue(this AttributeMetadata metadata)
        {
            if (metadata is IntegerAttributeMetadata) return (metadata as IntegerAttributeMetadata).MaxValue ?? -1;
            if (metadata is DecimalAttributeMetadata) return (metadata as DecimalAttributeMetadata).MaxValue ?? -1;
            if (metadata is DoubleAttributeMetadata) return (metadata as DoubleAttributeMetadata).MaxValue?.ToDecimal() ?? -1;
            if (metadata is MoneyAttributeMetadata) return (metadata as MoneyAttributeMetadata).MaxValue?.ToDecimal() ?? -1;

            return null;
        }

        public static decimal? MinValue(this AttributeMetadata metadata)
        {
            if (metadata is IntegerAttributeMetadata) return (metadata as IntegerAttributeMetadata).MinValue ?? -1;
            if (metadata is DecimalAttributeMetadata) return (metadata as DecimalAttributeMetadata).MinValue ?? -1;
            if (metadata is DoubleAttributeMetadata) return (metadata as DoubleAttributeMetadata).MinValue?.ToDecimal() ?? -1;
            if (metadata is MoneyAttributeMetadata) return (metadata as MoneyAttributeMetadata).MinValue?.ToDecimal() ?? -1;

            return null;
        }
      
    }

    public static class OptionSetMetadataExtensions
    {
        public static string DisplayName(this OptionSetMetadata metadata)
        {
            return NameHelper.GetProperVariableName(metadata.Name);
        }
    }
}
