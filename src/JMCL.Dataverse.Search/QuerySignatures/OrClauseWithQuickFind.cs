using Microsoft.Xrm.Sdk.Query;

namespace JMCL.Dataverse.Sdk.Utilities.Search
{
    public class OrClauseWithQuickFind : SearchQuerySignatureBase
    {
        public OrClauseWithQuickFind() : base()
        {
            this.FilterOperator = LogicalOperator.Or;
            this.RequireQuickFind = true;
        }
    }
}
