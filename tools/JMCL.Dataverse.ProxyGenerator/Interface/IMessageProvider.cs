using System;

namespace JMCL.Dataverse.ProxyGenerator
{
    public enum eMessageType { Verbose = 0, Info, Warning, Error }
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string MessageExtended { get; set; }
        public eMessageType MessageType { get; set; } = eMessageType.Info;
    }

    public delegate void MessageHandler(object sender, MessageEventArgs e);


    public interface IMessageProvider
    {
        event MessageHandler Message;
        
    }
}
