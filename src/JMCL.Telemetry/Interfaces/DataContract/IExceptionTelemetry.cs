using System;
using System.Collections.Generic;

namespace JMCL.Telemetry
{
    public interface IExceptionTelemetry : ITelemetry, IDataModelTelemetry<IExceptionDataModel>, ISupportProperties, ISupportMetrics, ISupportSampling
    {
        Exception Exception { get; }
        IList<IExceptionDetails> ExceptionDetails { get; }
    }
}
