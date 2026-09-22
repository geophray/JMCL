using System.Linq;

namespace JMCL.Dataverse.ProxyGenerator.Model
{
    public class MessageFieldModel
    {
        private readonly string TypeName;

        public MessageFieldModel(string name, string fullyQualifiedTypeName, bool isRequired)
        {
            Name = name;
            TypeName = fullyQualifiedTypeName.Split(',').FirstOrDefault().Split('.').LastOrDefault();
            IsRequired = isRequired;
        }

  
        public string Name { get; }
        
        public TypeModel Type
        {
            get
            {
                switch (TypeName.ToLower())
                {                    
                    default:
                        return new TypeModel
                        {
                            ExposedType = TypeName,
                            NativeType = TypeName,
                        };
                }
            }
        }

        public bool IsRequired { get; }
    }
}
