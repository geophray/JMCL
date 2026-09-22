using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator
{
    using JMCL.Dataverse.Sdk.Metadata;
    using Model;
    using Parser;
    using System;

    public class ProxyModelService : MessagingBase, IProxyModelService
    {      
        private readonly ITypeConverter TypeConverter;

        public ProxyModelService(ITypeConverter TypeConverter)
        {
            this.TypeConverter = TypeConverter ?? throw new ArgumentNullException("TypeConverter is required.");
        }

        public ProxyModel BuildModel(IEnumerable<EntityMetadata> entityMetadata, IEnumerable<SdkMessageMetadata> messageMetadata)
        {
            var proxyModel = new ProxyModel(this.TypeConverter);
            
            var entities = entityMetadata
                .Select(e => EntityParser.Parse(proxyModel, e))
                .OrderBy(e => e.DisplayName);

            var globals = proxyModel.GlobalOptionSets = GetGlobalOptionsFromEntities(entities)
                .OrderBy(o => o.DisplayName);                
                
            var messages = messageMetadata
                .Select(m => MessageParser.Parse( m))
                .OrderBy(m => m.SchemaName);

            proxyModel.Entities = entities;
            proxyModel.GlobalOptionSets = globals;
            proxyModel.Messages = messages;

            RaiseMessage("Generated Proxy Model...");

            return proxyModel;            
        }

       
        private IEnumerable<EnumModel> GetGlobalOptionsFromEntities(IEnumerable<EntityModel> entities)
        {
            var globalOptionSets = new List<EnumModel>();

            foreach (var e in entities)
            {
                foreach (var f in e.Fields.Where(f => f.Enum != null && f.Enum.IsGlobal == true))
                {
                    var globalEnum = globalOptionSets.Where(o => o.DisplayName == f.Enum.DisplayName).FirstOrDefault();

                    if(globalEnum is null)
                    {
                        globalOptionSets.Add(f.Enum);                        
                    }                    
                }
            }

            return globalOptionSets;
        }
    }
}
