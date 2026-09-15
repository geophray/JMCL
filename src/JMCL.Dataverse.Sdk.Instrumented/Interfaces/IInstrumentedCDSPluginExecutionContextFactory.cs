using System;
using Microsoft.Xrm.Sdk;

namespace JMCL.Dataverse.Sdk
{
    using JMCL.Core;
    using JMCL.Telemetry;

    public interface IInstrumentedCDSPluginExecutionContextFactory<T> where T :  IInstrumentedCDSPluginExecutionContext
    {
        T CreateCDSExecutionContext(IExecutionContext executionContext, IServiceProvider serviceProvider, IIocContainer container, IComponentTelemetryClient telemetryClient );
    }
}
