using System.Runtime.Serialization;

namespace JMCL.Azure.Secrets
{
    using JMCL.Core.Serialization;
    using JMCL.Core.RestClient;

    [DataContract]
    public class SecretList : ISerializedRESTResponse
    {        
        [DataMember]
        public SecretItem[] value { get; set; }
        [DataMember]
        public string nextLink { get; set; }

        public string ToString(IDataSerializer serializer)
        {
            return serializer.Serialize<SecretList>(this);
        }
    }
}
