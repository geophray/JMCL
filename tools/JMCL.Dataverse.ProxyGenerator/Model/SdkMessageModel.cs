using System.Collections.Generic;

namespace JMCL.Dataverse.ProxyGenerator.Model
{
    public class SdkMessageModel
    { 
        public SdkMessageModel(string schemaName, string displayName, string messageNamespace, IEnumerable<MessageFieldModel> requestFields, IEnumerable<MessageFieldModel> responseFields)
        {
            SchemaName = schemaName;
            DisplayName = displayName;
            MessageNamespace = messageNamespace;
            RequestFields = requestFields;
            ResponseFields = responseFields;
        }

        public string SchemaName { get; }
        public string DisplayName { get; }
        public string MessageNamespace { get; }
        public IEnumerable<MessageFieldModel> RequestFields { get;  }
        public IEnumerable<MessageFieldModel> ResponseFields { get;  }
    }
}
