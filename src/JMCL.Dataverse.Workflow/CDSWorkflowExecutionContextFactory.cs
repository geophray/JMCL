namespace JMCL.Dataverse.Sdk
{
    using System.Activities;
    using JMCL.Core;
    using Microsoft.Xrm.Sdk.Workflow;

    public class CDSWorkflowExecutionContextFactory : ICDSWorkflowExecutionContextFactory<ICDSWorkflowExecutionContext>
    {
        public ICDSWorkflowExecutionContext CreateCDSExecutionContext(IWorkflowContext executionContext, CodeActivityContext codeActivityContext, IIocContainer container)
        {
            return new CDSWorkflowExecutionContext(executionContext, codeActivityContext, container);
        }
    }
}
