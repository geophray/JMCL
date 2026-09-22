using System;
using System.Runtime.InteropServices;
using Microsoft.Xrm.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator.Model
{
    [Serializable]
    public class FieldModel
    {
        
        private readonly eCdsDataType DataType;
        
        public EntityModel EntityModel { get; }
        
        public string LogicalName { get; }
        public string SchemaName { get; }
        public string DisplayName { get; internal set; }
        public string PrivatePropertyName { get; }
        public bool IsAuditEnabled { get; }
        public bool IsPrimaryKey { get; }
        public bool IsPrimaryName { get; }
        public bool IsLookup { get; }
        public bool IsRequired { get; }
        public bool IsUpdatable { get; }    
        public bool IsCreateable { get; }
        public bool IsVirtual => DataType == eCdsDataType.Virtual;
        public bool IsReadOnly => !(IsCreateable || IsUpdatable);
        public int? MaxLength { get; }
        public decimal? Min { get; }
        public decimal? Max { get; }
               
        public EnumModel Enum { get; internal set; }

        private TypeModel _typeModel;
        public TypeModel Type
        {
            get
            {
                if (_typeModel is null)
                {
                    var enumTypeName = Enum?.Type ?? Enum?.FieldModel.EntityModel.ProxyModel.GetGlobalEnumType(this.Enum?.DisplayName);
                    _typeModel = this.EntityModel.ProxyModel.TypeConverter.GetTypeModel(this.DataType, enumTypeName);
                }
                        
                return _typeModel;
            }
        }
         

        public FieldModel(EntityModel parent, string logicalName, string schemaName, string displayName, string privatePropertyName, eCdsDataType dataType, bool isPrimaryKey, bool isPrimaryName, bool isLookup, bool isRequired, bool isUpdatable, bool isCreateable, decimal? minimumValue, decimal? maximumValue, int? maximumLength, bool isAuditEnabled)
        {
            this.DataType = dataType; ;
            this.EntityModel = parent;
            this.LogicalName = logicalName;
            this.SchemaName = schemaName;
            this.DisplayName = displayName;
            this.PrivatePropertyName = privatePropertyName;
            
            this.IsPrimaryKey = isPrimaryKey;
            this.IsPrimaryName = isPrimaryName;
            this.IsLookup = isLookup;
            this.IsRequired = isRequired;
            this.IsUpdatable = isUpdatable;
            this.IsCreateable = isCreateable;
            this.Min = minimumValue;
            this.Max = maximumValue;
            this.MaxLength = maximumLength;

            this.IsAuditEnabled = isAuditEnabled;
        }


    }
}
