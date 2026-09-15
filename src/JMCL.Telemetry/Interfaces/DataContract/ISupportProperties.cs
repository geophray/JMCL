using System.Collections.Generic;

namespace JMCL.Telemetry
{
    public interface ISupportProperties
    {
        IDictionary<string, string> Properties { get; }
    }
}
