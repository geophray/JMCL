using System;
using System.Collections.Generic;
using System.Linq;

namespace JMCL.Dataverse.ProxyGenerator.Model
{
    public class EntityModel 
    {   
        internal ProxyModel ProxyModel { get; }
        internal string PrimaryKeyAttribute { get; }
        internal string PrimaryNameAttribute { get; }
        public string LogicalName { get; }
        public string SchemaName { get; }
        public bool IsAuditEnabled { get; }
        
        public string DisplayName { get; }
        public string PluralName { get; }

        private IEnumerable<FieldModel> _fields;
        public IEnumerable<FieldModel> Fields
        {
            get { return _fields; }
            internal set
            {
                _fields = value;
                SetPrimaryKey();
                SetPrimaryName();
            }
        }
        public FieldModel PrimaryKey { get; private set; }
        public FieldModel PrimaryName { get; private set; }

        public IEnumerable<RelationshipModel> RelationshipsOneToMany { get; internal set; }
        public IEnumerable<RelationshipModel> RelationshipsManyToOne { get; internal set; }
        public IEnumerable<RelationshipModel> RelationshipsManyToMany { get; internal set; }


        public EntityModel(
            ProxyModel parent,
            string logicalName, 
            string schemaName, 
            string displayName, 
            string pluralName, 
            string primaryKey,
            string primaryName,
            bool isAuditEnabled)
        {
            this.ProxyModel = parent;
            this.LogicalName = logicalName;
            this.SchemaName = schemaName;
            this.DisplayName = displayName;
            this.PluralName = pluralName;
            this.PrimaryKeyAttribute = primaryKey;
            this.PrimaryNameAttribute = primaryName;
            this.IsAuditEnabled = isAuditEnabled;
        }

        private void SetPrimaryKey()
        {
            this.PrimaryKey = _fields?.Where(f => f.LogicalName == this.PrimaryKeyAttribute).FirstOrDefault();
        }

        private void SetPrimaryName()
        {
            this.PrimaryName = _fields?.Where(f => f.LogicalName == this.PrimaryNameAttribute).FirstOrDefault();
        }
        
       
    }
}
