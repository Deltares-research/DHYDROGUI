using DeltaShell.NGHS.IO;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    /// <summary>
    /// Reader for finding the relative grid file path in the htc file
    /// </summary>
    public class HtcFileReader : NGHSFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HtcFileReader));
        private readonly string filePath;
        private const string GridFileKeyword = "grid_file";
        private const string endOfHeaderKeyword = "time";
        
        public HtcFileReader(string filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>
        /// Using a pattern, the relative grid file path can be found in the header of the file
        /// </summary>
        /// <returns></returns>
        public string ReadGridFileNameWithExtension()
        {
            OpenInputFile(filePath);
            try
            {
                string line;

                while ((line = GetNextLine()) != null)
                {
                    string[] fields = GetKeyValueComment(line);
                    string foundKeyWordTrimmed = fields[0].Trim();
                    if (foundKeyWordTrimmed == GridFileKeyword)
                    {
                        return fields[1].Trim();
                    }
                    else if (foundKeyWordTrimmed.ToLower() == endOfHeaderKeyword)
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
           return ReaderHelper.GetKeyValueComment(line, LineNumber, InputFilePath);
        }
    }
}