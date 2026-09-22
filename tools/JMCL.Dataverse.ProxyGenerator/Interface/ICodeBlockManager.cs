namespace JMCL.Dataverse.ProxyGenerator
{
    public interface ICodeBlockManager
    {
        void StartFile(string name, bool includeHeader = true, bool includeFooter = true);
        void StartFooter(bool includeInDefault = true);
        void StartHeader(bool includeInDefault = true);
        void EndBlock();
        void Process(bool split = true, bool createDefault = false);
    }
}
