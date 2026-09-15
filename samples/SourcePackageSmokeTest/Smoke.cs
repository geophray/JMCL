using System;
using JMCL.Core;
using JMCL.Dataverse.Sdk;

namespace SourcePackageSmokeTest
{
    /// <summary>
    /// Touches one type from the directly referenced source package and one that
    /// can only arrive transitively. If either fails to resolve, the .Sources
    /// packaging is not delivering compilable source.
    /// </summary>
    internal static class Smoke
    {
        internal static Type[] Probe()
        {
            return new[]
            {
                // Transitive: JMCL.Core.IocContainer.Sources, via the dependency chain.
                typeof(IocContainer),
                typeof(IProcessExecutionContext),

                // Direct: JMCL.Dataverse.Sdk.Sources.
                typeof(CDSPlugin),
                typeof(CDSExecutionContext),
            };
        }
    }
}
