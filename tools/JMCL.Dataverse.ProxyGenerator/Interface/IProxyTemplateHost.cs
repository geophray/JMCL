using Microsoft.VisualStudio.TextTemplating;

namespace JMCL.Dataverse.ProxyGenerator
{
    public interface IProxyTemplateHost : ITextTemplatingEngineHost, ITextTemplatingSessionHost, IMessageProvider
    {
        string OutputPath { get; }
        string FileExtension { get; }
        void RaiseMessage(string message, string extendedMessage = "", eMessageType messageType = eMessageType.Info);
    }
}
