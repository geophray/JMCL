using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMCL.Dataverse.ProxyGenerator.Cmd
{
    public interface IDirectoryService
    {
        string GetApplicationDirectory();
        string SimpleSearch(string path, string search);
        List<string> Search(string path, string search);        
    }
}
