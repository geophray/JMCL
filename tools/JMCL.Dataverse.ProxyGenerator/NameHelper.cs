using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using JMCL.Dataverse.ProxyGenerator.Extensions;

namespace JMCL.Dataverse.ProxyGenerator
{
    public class NameHelper
    {
        public static string GetProperVariableName(string p)
        {
            if (string.IsNullOrWhiteSpace(p))
                return "Empty";

            return Clean(Capitalize(p, true));
        }

        

        public static string GetEntityPropertyPrivateName(string p)
        {
            return "_" + Clean(Capitalize(p, false));
        }
        private static string Clean(string p)
        {
            string result = "";
            if (!string.IsNullOrEmpty(p))
            {
                p = p.Trim();
                p = Normalize(p);

                if (!string.IsNullOrEmpty(p))
                {
                    StringBuilder sb = new StringBuilder();
                    int start = 0;
                    if (!char.IsLetter(p[0]))
                    {
                        if (p[0].IsCurrencySymbol())
                        {
                            sb.Append("Currency_");
                        }

                        else if (p[0].IsPercentSymbol())
                        {
                            sb.Append("Percent_");
                        }

                        else
                        {
                            sb.Append("_");
                        }
                    }

                    for (int i = start; i < p.Length; i++)
                    {
                        if ((char.IsDigit(p[i]) || char.IsLetter(p[i])) && !string.IsNullOrEmpty(p[i].ToString()))
                        {
                            sb.Append(p[i]);
                        }
                    }

                    result = sb.ToString();
                }                

                result = Regex.Replace(result, "[^A-Za-z0-9_]", "");
            }

            return result;
        }

        private static string Normalize(string regularString)
        {
            string normalizedString = regularString.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder(normalizedString);

            for (int i = 0; i < sb.Length; i++)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(sb[i]) == UnicodeCategory.NonSpacingMark)
                    sb.Remove(i, 1);
            }
            regularString = sb.ToString();

            return regularString.Replace("æ", "");
        }

        private static string CapitalizeWord(string p)
        {
            if (string.IsNullOrWhiteSpace(p))
                return "";

            return p.Substring(0, 1).ToUpper() + p.Substring(1);
        }

        private static string DecapitalizeWord(string p)
        {
            if (string.IsNullOrWhiteSpace(p))
                return "";

            return p.Substring(0, 1).ToLower() + p.Substring(1);
        }

        private static string Capitalize(string p, bool capitalizeFirstWord)
        {
            var parts = p.Split(' ', '_');

            for (int i = 0; i < parts.Count(); i++)
                parts[i] = i != 0 || capitalizeFirstWord ? CapitalizeWord(parts[i]) : DecapitalizeWord(parts[i]);

            return string.Join("_", parts);
        }

        
    }
}
