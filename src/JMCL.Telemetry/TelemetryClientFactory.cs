using System.Collections.Generic;
using JMCL.Telemetry.Client;

namespace JMCL.Telemetry
{
    public class TelemetryClientFactory : ITelemetryClientFactory
    {
        ITelemetryContext telemetryContext;
        public ITelemetryInitializerChain InitializerChain { get; private set; }

        public TelemetryClientFactory(ITelemetryContext context, ITelemetryInitializerChain telemetryInitializers)
        {
            this.telemetryContext = context;
            this.InitializerChain = telemetryInitializers;
        }

        public IComponentTelemetryClient BuildClient(string applicationName, ITelemetrySink telemetrySink, IDictionary<string, string> contextProperties = null)
        {
            return new ComponentTelemetryClient(applicationName, telemetrySink, telemetryContext.BuildNew(), InitializerChain, contextProperties);
        }
    }
}
