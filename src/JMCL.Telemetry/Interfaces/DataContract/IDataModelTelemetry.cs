using System;
using System.Collections.Generic;

namespace JMCL.Telemetry
{ 
    public interface IDataModelTelemetry
    {
        void SerializeData(ITelemetrySerializer serializer, IJsonWriter writer);
        string BaseType { get; }             
        
    }

    public interface IDataModelTelemetry<TData> : IDataModelTelemetry where TData : IDataModel
    {        
        TData Data { get; }
        
        IDataModelTelemetry<TData> DeepClone();
        
    }
}
