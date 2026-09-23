using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMCL.Dataverse.ProxyGenerator.Model
{
    public class PrimaryKeyModel
    {
        public string LogicalName { get; }
        public string DisplayName
        {
            get
            {
                return NameHelper.GetProperVariableName(this.LogicalName);
            }
        }

        public PrimaryKeyModel(string logicalName)
        {
            this.LogicalName = logicalName;
        }

        public static implicit operator string(PrimaryKeyModel primaryKeyModel)
        {
            return primaryKeyModel.LogicalName;
        }
    }
}
