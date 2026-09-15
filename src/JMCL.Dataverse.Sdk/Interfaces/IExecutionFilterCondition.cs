namespace JMCL.Dataverse.Sdk
{
    public interface IExecutionFilterCondition
    {
        void Invert();
        bool Test(ICDSPluginExecutionContext executionContext);
    }
}
