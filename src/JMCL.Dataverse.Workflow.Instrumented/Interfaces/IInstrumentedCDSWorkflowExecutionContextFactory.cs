namespace JMCL.Dataverse.Sdk
{
    using System.Activities;
    using JMCL.Core;
    using JMCL.Telemetry;
    using Microsoft.Xrm.Sdk.Workflow;

    public interface IInstrumentedCDSWorkflowExecutionContextFactory<T> where T : IInstrumentedCDSExecutionContext
    {
        T CreateCDSExecutionContext(IWorkflowContext executionContext, CodeActivityContext codeActivityContext, IIocContainer container, IComponentTelemetryClient telemetryClient);
    }
}
