using System;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.IniReaders
{
    public class SedMorIniReader : IniReader
    {
        /// <summary>
        /// Regular expression for a key/value/comment line, where key & value are a string without
        /// whitespace
        /// </summary>
        private const string KeyValueCommentPattern =
            @"\s*" +                 // pre-whitespace
            @"(?<key>[\w]+)" +       // key
            @"\s*=\s*" +             // =
            @"((#(?<value>.*?)#)|" + // value (delimited with # and allows spaces), or:
            @"(?<value>[^#\s]*))" +  //    value (no spaces)
            @"\s*" +                 // whitespace
            @"(?<comment>.*)?$";     // comment (including unit)

        /// <summary>
        /// Regular expression for a key/value/comment line, where key is a string without whitespace
        /// and value can contain whitespace. There can be no comments.
        /// </summary>
        private const string KeyValuePattern =
            @"\s*" +           // pre-whitespace
            @"(?<key>[\w]+)" + // key
            @"\s*=\s*" +       // =
            @"(?<value>.*)";   // value

        protected override string[] GetKeyValueComment(string lineContent)
        {
            var result = new string[3];
            if (LineNumber < 5) // in header (assume fixed header)
            {
                // it's a bit crude, but the first few lines (eg, the header) in the .sed & .mor file have a 
                // different parse format
                MatchCollection matches = RegularExpression.GetMatches(KeyValuePattern, lineContent);
                if (matches.Count == 0)
                {
                    throw new FormatException(string.Format("Invalid key-value line on line {0} in file {1}",
                                                            LineNumber, InputFilePath));
                }

                result[0] = matches[0].Groups["key"].Value.Trim();
                result[1] = matches[0].Groups["value"].Value.Trim();
                result[2] = "";
            }
            else
            {
                MatchCollection matches = RegularExpression.GetMatches(KeyValueCommentPattern, lineContent);
                if (matches.Count == 0)
                {
                    throw new FormatException(string.Format("Invalid key-value-comment line on line {0} in file {1}",
                                                            LineNumber, InputFilePath));
                }

                result[0] = matches[0].Groups["key"].Value.Trim();
                result[1] = matches[0].Groups["value"].Value.Trim();
                result[2] = matches[0].Groups["comment"].Value.Trim(); // Returns "" if comment group not matched
            }

            return result;
        }
    }
}