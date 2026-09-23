using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JMCL.Dataverse.ProxyGenerator.Extensions
{
    public static class IEnumerableStringExtensions
    {
        public static bool HasRegExMatch(this IEnumerable<string> patterns, string value)
        {
            foreach (var pattern in patterns.Where(s => s.StartsWith("^") && s.EndsWith("$")))
            {
                try { 
                    var regEx = new Regex(pattern, RegexOptions.IgnoreCase);
                    if (regEx.IsMatch(value))
                        return true;
                }
                catch { 
                }
            }

            return false;
        }
    }
}
