using System.IO;

namespace JMCL.Telemetry
{
    public interface IJsonWriterFactory
    {
        IJsonWriter BuildJsonWriter(TextWriter textWriter);
    }
}
