using Microsoft.Xrm.Sdk.Query;

namespace JMCL.Dataverse.Sdk.Utilities.Search
{
    public class OrClause : SearchQuerySignatureBase
    {
        public OrClause() : base()
        {
            this.FilterOperator = LogicalOperator.Or;            
        }
    }
}
