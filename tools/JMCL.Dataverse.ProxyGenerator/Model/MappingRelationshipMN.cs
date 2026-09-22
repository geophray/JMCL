using Microsoft.Xrm.Sdk.Metadata;


namespace JMCL.Dataverse.ProxyGenerator.Model
{
    public class MappingRelationshipMN
    {
        public CDSRelationshipAttribute Attribute { get; set; }

        public string DisplayName
        {
            get;
            set;
        }

        public string ForeignKey
        {
            get
            {
                return Helper.GetProperVariableName(Attribute.ToKey);
            }
        }

        public string PrivateName
        {
            get;
            set;
        }

        public string Type
        {
            get
            {
                return Helper.GetProperVariableName(Attribute.FromEntity);
            }
        }

        public static MappingRelationshipN1 Parse(ManyToManyRelationshipMetadata rel)
        {
            return new MappingRelationshipN1
            {
                Attribute = new CDSRelationshipAttribute
                {
                    //FromEntity = rel.ReferencedEntity,
                    //FromKey = rel.ReferencedAttribute,
                    //ToEntity = rel.ReferencingEntity,
                    //ToKey = rel.ReferencingAttribute,
                    IntersectingEntity = rel.IntersectEntityName
                },
                DisplayName = Helper.GetProperVariableName(rel.SchemaName),
                PrivateName = "_nn" + Helper.GetEntityPropertyPrivateName(rel.SchemaName)
            };
        }
    }
}
