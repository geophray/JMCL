using System;
using Microsoft.Xrm.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator.Parser
{
    using Extensions;
    using Model;

    public class RelationshipParser
    {
        public static RelationshipModel Parse(EntityModel parent, eRelationshipType type, RelationshipMetadataBase metadata)
        {
            switch (type)
            {
                case eRelationshipType.OneToMany:
                    var rel1N = (OneToManyRelationshipMetadata)metadata;
                    return new RelationshipModel(
                        parent,
                        schemaName: metadata.SchemaName,
                        displayName: metadata.DisplayName(),
                        privateName: "_n1" + metadata.PrivatePropertyName(),
                        fromEntityName: rel1N.ReferencedEntity,
                        fromFieldName: rel1N.ReferencedAttribute,
                        toEntityName: rel1N.ReferencingEntity,
                        toFieldName: rel1N.ReferencingAttribute,
                        intersectingEntityName: string.Empty);

                case eRelationshipType.ManyToOne:
                    var relN1 = (OneToManyRelationshipMetadata)metadata;
                    return new RelationshipModel(
                        parent,
                        schemaName: metadata.SchemaName,
                        displayName: metadata.DisplayName(),
                        privateName: "_1n" + metadata.PrivatePropertyName(),
                        fromEntityName: relN1.ReferencingEntity,
                        fromFieldName: relN1.ReferencingAttribute,
                        toEntityName: relN1.ReferencedEntity,
                        toFieldName: relN1.ReferencedAttribute,
                        intersectingEntityName: string.Empty);

                case eRelationshipType.ManyToMany:
                    var relMN = (ManyToManyRelationshipMetadata)metadata;
                    return new RelationshipModel(
                        parent,
                        schemaName: metadata.SchemaName,
                        displayName: metadata.DisplayName(),
                        privateName: "_mn" + metadata.PrivatePropertyName(),
                        fromEntityName: relMN.Entity1LogicalName,
                        fromFieldName: relMN.Entity1IntersectAttribute,
                        toEntityName: relMN.Entity2LogicalName,
                        toFieldName: relMN.Entity2IntersectAttribute,
                        intersectingEntityName: relMN.IntersectEntityName);
            }

            throw new Exception("Unsupported relationship type.");
        }
    }
}
