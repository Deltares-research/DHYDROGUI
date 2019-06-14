using System;
using System.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public static class HtcFile
    {
        public static string GetCorrespondingGridFilePath(string filePath)
        {
            if (filePath == null)
            {
                throw new InvalidOperationException(("Heat flux file path is not valid"));
            }

                string fullPath = Path.GetFullPath(filePath);
                var htcFileReader = new HtcFileReader(fullPath);
                string gridFileName = htcFileReader.ReadGridFileNameWithExtension();

                if (gridFileName == null)
                {
                    throw new InvalidOperationException(("Relative Grid file path is missing in the *.htc file"));
                }

                string htcDir = Path.GetDirectoryName(fullPath);
                if (htcDir == null)
                {
                    throw new InvalidOperationException(("Directory of heat flux file is not valid"));
                }

                return Path.Combine(htcDir, gridFileName);
        }
    }
}