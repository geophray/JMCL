using Microsoft.Xrm.Sdk.Query;

namespace JMCL.Dataverse.Sdk.Utilities.Search
{
    public class AndClauseWithQuickFind : SearchQuerySignatureBase
    {
        public AndClauseWithQuickFind() : base()
        {
            this.FilterOperator = LogicalOperator.And;
            this.RequireQuickFind = true;
        }
    }
}
