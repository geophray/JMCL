using System;


namespace JMCL.Dataverse.FieldEncryption
{
    using JMCL.Core;

    /// <summary>
    /// Interface for evaluating if a user is assigned to one or more roles.
    /// </summary>
    public interface IUserRoleEvaluator
    {
        bool IsAssignedRole(IProcessExecutionContext executionContext, Guid userId, params string[] roles);
    }
}
