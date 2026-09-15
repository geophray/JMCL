namespace JMCL.Dataverse.Sdk
{
    using JMCL.Telemetry;
    

    public interface IInstrumentedCDSExecutionContext : ICDSExecutionContext
    {
        IComponentTelemetryClient TelemetryClient { get; }
        ITelemetryFactory TelemetryFactory { get; }
        void SetAlternateDataKey(string name, string value);        
    }
}
