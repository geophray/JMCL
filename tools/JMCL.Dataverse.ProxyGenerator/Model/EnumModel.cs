using System.Collections.Generic;

namespace JMCL.Dataverse.ProxyGenerator.Model
{
    
    public class EnumModel 
    {
        public FieldModel FieldModel { get; }
        public bool IsGlobal { get; }
        public string DisplayName { get; }
        public string SchemaName { get; }
        public IEnumerable<EnumModelItem> Items { get; }
        public string Type { get; private set; }

        public EnumModel(FieldModel parent, string displayName, string schemaName, bool isGlobal, IEnumerable<EnumModelItem> items)
        {
            this.FieldModel = parent;
            this.DisplayName = displayName;
            this.SchemaName = schemaName;
            this.IsGlobal = isGlobal;
            this.Items = items;
        }

        public void SetType(string value)
        {
            this.Type = value;
        }
       
    }

    
    public class EnumModelItem
    {        
        public string DisplayName { get; internal set; }
        public int Value { get; internal set; }          
    }
}
