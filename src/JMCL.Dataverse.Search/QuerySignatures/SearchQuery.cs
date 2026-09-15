using Microsoft.Xrm.Sdk.Query;

namespace JMCL.Dataverse.Sdk.Utilities.Search
{
    public class SearchQuery : SearchQuerySignatureBase
    {
        public SearchQuery() : base()
        {
            base.RequireQuickFind = true;
        }

        public override bool Test(FilterExpression filter)
        {

            return base.Test(filter);
        }

        
    }
}
