using System.Collections.Generic;

namespace JMCL.Telemetry
{
    public interface IEventDataModel : IDataModel
    {
        string name { get; set; }
        IDictionary<string, double> measurements { get; set; }
        


    }
}
