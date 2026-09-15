using System;
using System.Collections.Generic;

namespace JMCL.Dataverse.Sdk.Utilities.Search
{
    public interface IQuickFindLinkedEntity
    {
        Guid[] GetLinkedIds(ICDSExecutionContext executionContext, string searchTerm, bool useElevatedAccess);
    }
}
