using System.Linq;

namespace JMCL.Dataverse.ProxyGenerator.Parser
{
    using JMCL.Dataverse.Sdk.Metadata;
    using Model;
    using System.Linq;

    public class MessageParser
    {
        public static SdkMessageModel Parse(SdkMessageMetadata message)
        {
            var schemaName = message.Name;
            var displayName = NameHelper.GetProperVariableName(schemaName);
            var messageNamespace = message.MessagePairMetadata.FirstOrDefault()?.MessageNamespace;
            var requestFields = message.MessagePairMetadata.FirstOrDefault()?.RequestMetadata.FieldMetadata.Select(e => new MessageFieldModel(e.Name, e.CLRFormatter, e.IsOptional == false)).ToArray();
            var responseFields = message.MessagePairMetadata.FirstOrDefault()?.ResponseMetadata.FieldMetadata.Select(e => new MessageFieldModel(e.Name, e.CLRFormatter, false));

            return new SdkMessageModel(schemaName, displayName, messageNamespace, requestFields, responseFields);
        }
    }
}
