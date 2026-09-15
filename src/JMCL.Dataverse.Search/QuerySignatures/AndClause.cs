using Microsoft.Xrm.Sdk.Query;

namespace JMCL.Dataverse.Sdk.Utilities.Search
{
    public class AndClause : SearchQuerySignatureBase
    {
        public AndClause() : base()
        {
            this.FilterOperator = LogicalOperator.And;            
        }
    }
}
