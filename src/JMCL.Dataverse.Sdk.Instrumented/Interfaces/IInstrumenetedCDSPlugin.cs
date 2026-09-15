
namespace JMCL.Dataverse.Sdk
{
    using JMCL.Telemetry;

    public interface IInstrumenetedCDSPlugin : ICDSPlugin
    {
        ITelemetrySink TelemetrySink { get; }

        bool ConfigureTelemetrySink(ICDSPluginExecutionContext processContext);

        bool TrackExecutionPerformance { get; set; }

        bool FlushTelemetryAfterExecution { get; set; }

        string DefaultInstrumentationKey { get; set; }

        string InstrumentationVariableName { get; }
    }
}
