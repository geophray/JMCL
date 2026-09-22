using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator.Model
{
    [Serializable]
    public class MappingRelationshipN1
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
            get;
            set;
        }

        public string Type
        {
            get
            {
                return Helper.GetProperVariableName(Attribute.ToEntity);
            }
        }

        public static MappingRelationshipN1 Parse(OneToManyRelationshipMetadata rel, FieldModel[] properties)
        {
            var propertyName =
                properties.First(p => p.Attribute.LogicalName.ToLower() == rel.ReferencingAttribute.ToLower()).DisplayName;

            return new MappingRelationshipN1
            {
                Attribute = new CDSRelationshipAttribute
                {
                    ToEntity = rel.ReferencedEntity,
                    ToKey = rel.ReferencedAttribute,
                    FromEntity = rel.ReferencingEntity,
                    FromKey = rel.ReferencingAttribute,
                    IntersectingEntity = ""
                },
                DisplayName = Helper.GetProperVariableName(rel.SchemaName) +"_N1",
                PrivateName = "_n1"+ Helper.GetEntityPropertyPrivateName(rel.SchemaName),
                ForeignKey = propertyName
            };
        }
    }
}
