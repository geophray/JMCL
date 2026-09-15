using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace JMCL.Dataverse.Sdk
{
    public interface ICDSTelemetryPropertyManager
    {
        IDictionary<string, string> CreatePropertiesDictionary(string className, IExecutionContext executionContext);

    }
}
