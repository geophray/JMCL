using System;
using Microsoft.Xrm.Sdk;

namespace JMCL.Dataverse.Sdk
{
    using JMCL.Core;
    using JMCL.Telemetry;

    /// <summary>
    /// Factory for generating <see cref="InstrumentedCDSPluginExecutionContext"/>.
    /// </summary>
    public class InstrumentedCDSPluginExecutionContextFactory : IInstrumentedCDSPluginExecutionContextFactory<IInstrumentedCDSPluginExecutionContext>
    {  
        public IInstrumentedCDSPluginExecutionContext CreateCDSExecutionContext(IExecutionContext executionContext, IServiceProvider serviceProvider, IIocContainer container, IComponentTelemetryClient telemetryClient)
        {
            return new InstrumentedCDSPluginExecutionContext(serviceProvider, container, executionContext as IPluginExecutionContext, telemetryClient);
        }

       
    }
}
