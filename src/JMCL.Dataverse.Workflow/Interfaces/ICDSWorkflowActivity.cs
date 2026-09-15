namespace JMCL.Dataverse.Sdk
{
    using JMCL.Core;

    public interface ICDSWorkflowActivity
    {
        IIocContainer Container { get; }  
        
        void ExecuteInternal(ICDSWorkflowExecutionContext context);       
    }
}
