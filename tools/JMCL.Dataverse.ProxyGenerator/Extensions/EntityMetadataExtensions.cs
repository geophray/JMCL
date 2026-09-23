using Microsoft.Xrm.Sdk.Metadata;


namespace JMCL.Dataverse.ProxyGenerator.Extensions
{
    public static class EntityMetadataExtensions
    {
        public static string NormalizedName(this EntityMetadata metadata)
        {
            return NameHelper.GetProperVariableName(metadata.SchemaName);
        }

        public static string PluralName(this EntityMetadata metadata)
        {
            var entityName = metadata.SchemaName;

            if (entityName.EndsWith("y"))
                return entityName.Substring(0, entityName.Length - 1) + "ies";

            if (entityName.EndsWith("s"))
                return entityName;

            return entityName + "s";
        }

        public static bool IsAuditable(this EntityMetadata metadata)
        {
            return metadata.IsAuditEnabled?.Value ?? false;
        }
    }
}
