using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JMCL.Dataverse.ProxyGenerator.Extensions
{
    public static class RelationshipMetadataExtensions
    {
        public static string DisplayName(this RelationshipMetadataBase metadata)
        {
            return NameHelper.GetProperVariableName(metadata.SchemaName);
        }

        public static string PrivatePropertyName(this RelationshipMetadataBase metadata)
        {
            return NameHelper.GetEntityPropertyPrivateName(metadata.SchemaName);
        }
    }
}
