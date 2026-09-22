using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator.Model
{
    [Serializable]
    public class MappingRelationship1N
    {
        public CDSRelationshipAttribute Attribute { get; set; }

        public string DisplayName
        {
            get;
            set;
        }

        public string ForeignKey
        {
            get;
            set;
        }

        public string PrivateName
        {
            get; set;
        }
        
        public string Type
        {
            get
            {
                return Helper.GetProperVariableName(Attribute.ToEntity);
            }
        }

        public static MappingRelationship1N Parse(OneToManyRelationshipMetadata rel, CDSField[] properties)
        {
            var propertyName =
                properties.First(p => p.Attribute.LogicalName.ToLower() == rel.ReferencedAttribute.ToLower()).DisplayName;

            return new MappingRelationship1N
            {
                Attribute = new CDSRelationshipAttribute
                {
                    FromEntity = rel.ReferencedEntity,
                    FromKey = rel.ReferencedAttribute,
                    ToEntity = rel.ReferencingEntity,
                    ToKey = rel.ReferencingAttribute,
                    IntersectingEntity = ""
                },
                ForeignKey = propertyName,
                DisplayName = Helper.GetPluralName(Helper.GetProperVariableName(rel.SchemaName)),
                PrivateName = Helper.GetEntityPropertyPrivateName(rel.SchemaName)
            };
        }
    }
}
