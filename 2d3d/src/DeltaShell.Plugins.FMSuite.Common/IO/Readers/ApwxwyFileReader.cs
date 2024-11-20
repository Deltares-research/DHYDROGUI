using System;
using DeltaShell.NGHS.IO;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    public class ApwxwyFileReader : NGHSFileBase
    {
        private const string GridFileIdentifier = "grid_file";
        private static readonly ILog Log = LogManager.GetLogger(typeof(ApwxwyFileReader));
        private readonly string filePath;

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
                    string[] fields = GetKeyValueComment(line);
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
                return ReaderHelper.GetKeyValueComment(line, LineNumber, InputFilePath);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("During reading apwxwy file: {0}", ex.Message);
                return new string[3]
                {
                    "a",
                    "b",
                    "c"
                };
            }
        }
    }
}