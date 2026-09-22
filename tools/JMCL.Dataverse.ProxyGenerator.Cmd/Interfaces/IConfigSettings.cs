using JMCL.Dataverse.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMCL.Dataverse.ProxyGenerator.Cmd
{
    public interface IConfigSettings : ISettings
    {
        string ConfigurationPath { get; set; }
    }
}
