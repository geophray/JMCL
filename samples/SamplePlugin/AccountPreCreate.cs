using JMCL.Dataverse.Sdk;

namespace JMCL.Samples.Plugins
{
    /// <summary>
    /// Minimal plug-in demonstrating the JMCL registration model: the constructor
    /// declares which events this assembly handles, and the framework dispatches
    /// to the handler rather than the plug-in switching on context.MessageName.
    /// </summary>
    public class AccountPreCreate : CDSPlugin
    {
        public AccountPreCreate(string unsecureConfig, string secureConfig)
            : base(unsecureConfig, secureConfig)
        {
            RegisterEventHandler(
                entityName: "account",
                messageName: MessageNames.Create,
                stage: ePluginStage.PreOperation,
                handler: OnAccountPreCreate);
        }

        private void OnAccountPreCreate(ICDSPluginExecutionContext context)
        {
            context.Trace("JMCL sample plug-in executing for account pre-create.");

            // The execution context exposes the platform-agnostic process model,
            // so this handler could be lifted into an Azure Function unchanged.
            var target = context.TargetEntity;
            if (target != null)
            {
                context.Trace("Target logical name: {0}", target.LogicalName);
            }
        }
    }
}
