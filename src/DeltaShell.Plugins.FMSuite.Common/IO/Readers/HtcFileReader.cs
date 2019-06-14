using System;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    public class HtcFileReader : NGHSFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HtcFileReader));
        private readonly string filePath;
        private const string GridFileIdentifier = "grid_file";
        private const string KeyValueCommentPattern = @"^\s*(?<key>[^=\s]+)\s*=\s*(?<value>[^#=]*)(#(?<comment>.*))?$";

        public HtcFileReader(string filePath)
        {
            this.filePath = filePath;
        }

        public string ReadGridFileNameWithExtension()
        {
            OpenInputFile(filePath);
            try
            {
                string line;

                while ((line = GetNextLine()) != null)
                {
                    string[] fields = GetKeyValueComment(line);
                    if (fields[0].Trim() == GridFileIdentifier)
                    {
                        return fields[1].Trim();
                    }
                    else if (fields[0].Trim().ToLower() == "time")
                    {
                        break;
                    }

                }
            }
            finally
            {
                CloseInputFile();
            }
            return null;
        }

        private string[] GetKeyValueComment(string line)
        {

            var result = new string[3];

            MatchCollection matches = RegularExpression.GetMatches(KeyValueCommentPattern, line);
            if (matches.Count == 0)
            {
                throw new FormatException(string.Format("Invalid key-value-comment line on line {0} in file {1}",
                                                        LineNumber, InputFilePath));
            }

            result[0] = matches[0].Groups["key"].Value.Trim();
            result[1] = matches[0].Groups["value"].Value.Trim();
            result[2] = matches[0].Groups["comment"].Value.Trim(); // Returns "" if comment group not matched

            return result;
        }
    }
}