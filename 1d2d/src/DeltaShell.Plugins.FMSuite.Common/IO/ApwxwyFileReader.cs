using System;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class ApwxwyFileReader : NGHSFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ApwxwyFileReader));
        private readonly string filePath;
        private const string GridFileIdentifier = "grid_file";
        private const string KeyValueCommentPattern = @"^\s*(?<key>[^=\s]+)\s*=\s*(?<value>[^#=]*)(#(?<comment>.*))?$";

        public ApwxwyFileReader(string filePath)
        {
            this.filePath = filePath;
        }

        public string ReadGridFileNameWithExtension()
        {
            try
            {
                OpenInputFile(filePath);
                string line;

                while ((line = GetNextLine()) != null)
                {
                    var fields = GetKeyValueComment(line);
                    if (fields[0].Trim() == GridFileIdentifier)
                    {
                        CloseInputFile();
                        return fields[1].Trim();
                    }
                }
                CloseInputFile();
            }
            catch (Exception)
            {
                Log.ErrorFormat("File at '{0}' was not found.", filePath);
            }
            return null;
        }

        private string[] GetKeyValueComment(string line)
        {
            try
            {
                var result = new string[3];

                var matches = RegularExpression.GetMatches(KeyValueCommentPattern, line);
                if (matches.Count == 0) throw new FormatException(String.Format("Invalid key-value-comment line on line {0} in file {1}",
                    LineNumber, InputFilePath));

                result[0] = matches[0].Groups["key"].Value.Trim();
                result[1] = matches[0].Groups["value"].Value.Trim();
                result[2] = matches[0].Groups["comment"].Value.Trim(); // Returns "" if comment group not matched

                return result;
            }
            catch (Exception ex)
            {
                Log.WarnFormat("During reading apwxwy file: {0}", ex.Message);
                return new string[3] {"a","b","c"};
            }
        }
    }
}
