using System;
using System.IO;
using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    /// <summary>
    /// Reader for finding the relative grid file path in the htc file.
    /// </summary>
    public class HtcFileReader : NGHSFileBase
    {
        private const string GridFileKeyword = "grid_file";
        private const string endOfHeaderKeyword = "time";
        private readonly string filePath;

        public HtcFileReader(string filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>
        /// Using a pattern, the relative grid file path can be found in the header of the file
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Gridded heat flux file path is null</exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="IOException">
        /// <paramref name="filePath"/> includes an incorrect or invalid syntax for file name,
        /// directory name, or volume label.
        /// </exception>
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