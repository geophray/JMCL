using System;

namespace JMCL.Core.Net
{
    using JMCL.Telemetry;

    public interface IInstrumentedWebRequestFactory
    {
        IWebRequest CreateWebRequest(ITelemetryClient telemetryClient, Uri address, string telemetryTag = null);


        IWebRequest CreateWebRequest(ITelemetryClient telemetryClient, IAPIEndpoint endpoint, string telemetryTag = null);
        
    }
}
