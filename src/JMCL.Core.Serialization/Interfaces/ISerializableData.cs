namespace JMCL.Core.Serialization
{
    public interface ISerializableData
    {
        string ToString(IDataSerializer serializer);
    }
}
