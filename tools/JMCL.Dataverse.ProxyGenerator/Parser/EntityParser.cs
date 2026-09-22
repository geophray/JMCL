using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator.Parser
{
    using Extensions;
    using Model;

    public class EntityParser
    {
        public static EntityModel Parse(ProxyModel parent, EntityMetadata metadata)
        {
            var entityModel = new EntityModel(
                parent,
                metadata.LogicalName,
                metadata.SchemaName,
                metadata.NormalizedName(),
                metadata.PluralName(),
                metadata.PrimaryIdAttribute,
                metadata.PrimaryNameAttribute,
                metadata.IsAuditable()
                );

            var fields = metadata.Attributes               
               .Select(a => FieldParser.Parse(entityModel, a)).ToList();

            fields.ForEach(f =>
            {
                if (f.DisplayName == entityModel.DisplayName)
                    f.DisplayName += "1";
            });
          
            fields = fields.OrderBy(f => f.DisplayName).ToList();

            entityModel.Fields = fields;

            entityModel.RelationshipsOneToMany = metadata.OneToManyRelationships.Select(r => RelationshipParser.Parse(entityModel, eRelationshipType.OneToMany, r)).OrderBy(r => r.DisplayName);
            entityModel.RelationshipsManyToOne = metadata.ManyToOneRelationships.Select(r => RelationshipParser.Parse(entityModel, eRelationshipType.ManyToOne, r)).OrderBy(r => r.DisplayName);
            entityModel.RelationshipsManyToMany = metadata.ManyToManyRelationships.Select(r => RelationshipParser.Parse(entityModel, eRelationshipType.ManyToMany, r)).OrderBy(r => r.DisplayName);

            return entityModel;
        }
    }
}
