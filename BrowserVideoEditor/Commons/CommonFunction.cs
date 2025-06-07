using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserVideoEditor.Commons
{
    public class CommonFunction
    {
        public static List<string> SafeSplitString(string input, char[] delimiters = null)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            if (delimiters == null || delimiters.Length == 0)
                delimiters = new char[] { ',' }; // default delimiter

            return input
                .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
    }
}
