using System;
using System.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    /// <summary>
    /// HtcFile.GetCorrespondingGridFilePath checks the input and asks the htcFileReader for the relative path,
    /// written in the htc file. Anticipates if relative path can not be found (null) and formulates the absolute path
    /// </summary>
    public static class HtcFile
    {
        public static string GetCorrespondingGridFilePath(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("Heat flux file path is not valid");
            }

            string htcDir = Path.GetDirectoryName(filePath);
            if (htcDir == null)
            {
                throw new InvalidOperationException("Directory of heat flux file is not valid");
            }

            var htcFileReader = new HtcFileReader(filePath);
            string gridFileName = htcFileReader.ReadGridFileNameWithExtension();

            if (gridFileName == null)
            {
                throw new InvalidOperationException("Relative Grid file path is missing in the *.htc file");
            }

            return Path.Combine(htcDir, gridFileName);
        }
    }
}