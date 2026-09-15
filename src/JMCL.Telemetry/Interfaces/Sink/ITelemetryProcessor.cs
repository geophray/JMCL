namespace JMCL.Telemetry
{
    public interface ITelemetryProcessor
    {
        void Process(ITelemetry telemetryItem);
    }
}
