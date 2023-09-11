using System;
using System.IO;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// Class which provides which version of the definitions should be used.
    /// </summary>
    public class DefinitionsVersionProvider
    {
        private readonly ILogHandler logHandler;

        public DefinitionsVersionProvider(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }
        /// <summary>
        /// Gets the name of the definition version based on its input argument.
        /// </summary>
        /// <param name="gwswFileDirectory">The directory to determine
        /// the definition version for.</param>
        /// <returns>A string with the definition version</returns>
        public string GetDefinitionVersionName(string gwswFileDirectory)
        {
            // In the new GWSW format (1.5) Verbinding.csv has a column named
            // 'AAN_PRO'. We use this file and column to determine the version
            // of GWSW files. See issue FM1D2D-502.
            string path = gwswFileDirectory + @"\Verbinding.csv";

            string header = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    header = reader.ReadLine() ?? "";
                }
            }
            catch (Exception)
            {
                logHandler.ReportWarningFormat("Can't determine the Gwsw file format. Please select a folder with a valid Verbinding.csv file.");
            }

            return header.Contains("AAN_PRO") ? "GWSWDefinition1_5" : "GWSWDefinition";
        }
    }
}