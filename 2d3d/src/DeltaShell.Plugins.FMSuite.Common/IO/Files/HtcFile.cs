using System;
using System.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    /// <summary>
    /// HtcFile responsible for formulating the absolute grid file path and for testing the expectations for the output of the
    /// htcFileReader.
    /// </summary>
    public static class HtcFile
    {
        /// <summary>
        /// HtcFile.GetCorrespondingGridFilePath asks the htcFileReader for the relative path,
        /// written in the htc file and formulates the absolute path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Gridded heat flux file path is null</exception>
        /// <exception cref="InvalidOperationException">
        /// Directory of gridded heat flux file is not valid
        /// or
        /// Relative Grid file path is missing in the *.htc file
        /// </exception>
        public static string GetCorrespondingGridFilePath(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException($"Heat flux file path {0} is not valid", filePath);
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