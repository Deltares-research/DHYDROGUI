using System;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    public static class ReaderHelper
    {
        private const string KeyValueCommentPattern = @"^\s*(?<key>[^=\s]+)\s*=\s*(?<value>[^#=]*)(#(?<comment>.*))?$";
        public static string[] GetKeyValueComment(string line, int lineNumber, string inputFilePath)
        {

            var result = new string[3];

            MatchCollection matches = RegularExpression.GetMatches(KeyValueCommentPattern, line);
            if (matches.Count == 0)
            {
                throw new FormatException(
                    $"Invalid key-value-comment line on line {lineNumber} in file {inputFilePath}");
            }

            result[0] = matches[0].Groups["key"].Value.Trim();
            result[1] = matches[0].Groups["value"].Value.Trim();
            result[2] = matches[0].Groups["comment"].Value.Trim(); // Returns "" if comment group not matched

            return result;
        }
    }
}