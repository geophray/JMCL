using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace JMCL.Dataverse.Sdk
{  
    public interface IQueryExpressionBuilder<E> : IFluentQuery<IQueryExpressionBuilder<E>,E> where E : Entity 
    {
        IQueryExpressionBuilder<E> WithSearchValue(string searchValue);
        QueryExpression Build();
    }    
}
