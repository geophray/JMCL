using System.Collections.Generic;

namespace JMCL.Telemetry
{
    public interface ISupportMetrics
    {
        IDictionary<string, double> Metrics { get; }
    }
}
