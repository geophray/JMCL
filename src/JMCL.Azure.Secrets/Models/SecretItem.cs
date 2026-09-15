using System.Runtime.Serialization;

namespace JMCL.Azure.Secrets
{
    [DataContract]
    public class SecretItem
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public SecretAttributes attributes { get; set; }
    }
}
