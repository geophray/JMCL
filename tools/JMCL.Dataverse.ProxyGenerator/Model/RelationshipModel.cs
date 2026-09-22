
namespace JMCL.Dataverse.ProxyGenerator.Model
{
    public class RelationshipModel
    {
        internal readonly EntityModel ParentEntity;
      
        public string DisplayName { get; }
        public string PrivateName { get; }
        public string SchemaName { get; }
        public string ForiegnKey { get; }
        public EntityModel ReturnEntity => this.ParentEntity.ProxyModel.GetEntityModelByLogicalName(this.ToEntity);
        public string IntersectEntity { get; }
        public string FromEntity { get; }
        public string FromField { get; }
        public string ToEntity { get; }
        public string ToField { get; }
        
        public RelationshipModel(EntityModel parent, string schemaName, string displayName, string privateName, string fromEntityName, string fromFieldName, string toEntityName, string toFieldName, string intersectingEntityName)
        {
            ParentEntity = parent;
            SchemaName = schemaName;
            DisplayName = displayName;
            PrivateName = privateName;
            FromEntity = fromEntityName;
            FromField = fromFieldName;
            ToEntity = toEntityName;
            ToField = toFieldName;
            IntersectEntity = intersectingEntityName;
        }
    }
}
