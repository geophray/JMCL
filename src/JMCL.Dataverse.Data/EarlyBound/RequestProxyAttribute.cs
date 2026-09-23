using System;

namespace JMCL.Dataverse.Sdk.EarlyBound
{
    /// <summary>
    /// Marks a generated <see cref="Microsoft.Xrm.Sdk.OrganizationRequest"/> proxy class
    /// with the Dataverse SDK message schema name it wraps.
    /// </summary>
    /// <remarks>
    /// Added for the T4 template ported alongside JMCL.Dataverse.ProxyGenerator
    /// (fork-plan.md 11.1a). The template solution_template's Plugins/ProxyTemplate.t4
    /// already decorates generated request/response classes with [RequestProxy]/
    /// [ResponseProxy], but no version of the upstream codebase -- ITT ADO,
    /// Scott Colson's public original, or the previously-ported JMCL.Dataverse.Data --
    /// ever defined these attribute classes. actionsToInclude is empty in every
    /// proxies.json seen so far, so the gap was never actually hit at generation
    /// time. Defined here as a plain marker so the customized template's output
    /// compiles once a project adds message classes to actionsToInclude.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RequestProxyAttribute : Attribute
    {
        public RequestProxyAttribute(string schemaName)
        {
            SchemaName = schemaName;
        }

        public string SchemaName { get; }
    }

    /// <summary>
    /// Marks a generated <see cref="Microsoft.Xrm.Sdk.OrganizationResponse"/> proxy class
    /// with the Dataverse SDK message schema name it wraps. See
    /// <see cref="RequestProxyAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ResponseProxyAttribute : Attribute
    {
        public ResponseProxyAttribute(string schemaName)
        {
            SchemaName = schemaName;
        }

        public string SchemaName { get; }
    }
}
