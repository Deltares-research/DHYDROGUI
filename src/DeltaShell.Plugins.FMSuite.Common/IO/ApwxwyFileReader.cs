using System;
using DeltaShell.NGHS.IO;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class ApwxwyFileReader : NGHSFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ApwxwyFileReader));
        private readonly string filePath;
        private const string GridFileIdentifier = "grid_file";
        
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
                    var fields = line.Split('=');
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
    }
}
