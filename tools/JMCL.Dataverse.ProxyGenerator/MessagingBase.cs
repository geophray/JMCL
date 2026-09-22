namespace JMCL.Dataverse.ProxyGenerator
{   
    public abstract class MessagingBase : IMessageProvider
    {
        public event MessageHandler Message;

        protected void RaiseMessage(string message, string extendedMessage = "", eMessageType messageType = eMessageType.Info)
        {
            if (this.Message != null)
            {
                Message(this, new MessageEventArgs { Message = message, MessageExtended = extendedMessage, MessageType = messageType });
            }
        }
    }
}
