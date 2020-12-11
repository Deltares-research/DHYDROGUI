using System.IO;
using System.Linq;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    /// <summary>
    /// The AcceptanceModelExportHelper provides functionality for exporting
    /// the dia and log files used within TeamCity to generate reports.
    /// </summary>
    public static class AcceptanceModelExportHelper
    {
        /// <summary>
        /// Export an empty log file based upon the <paramref name="config"/>.
        /// </summary>
        /// <param name="config"> The configuration. </param>
        public static void ExportEmptyLogFile(AcceptanceModelExportResultConfig config)
        {
            string content = config.HasValidated
                                 ? "No diagnostic file found for this run."
                                 : "** ERROR : Failed to validate the model, no .dia file has been produced.";

            File.WriteAllText(Path.Combine(AcceptanceModelExportResultConfig.Delft3DfmExportDirectory,
                                           $"{config.OutputName}.dia"),
                              content);
        }

        /// <summary>
        /// Export the log file of stand-alone FM model based up on the <paramref name="config"/>.
        /// </summary>
        /// <param name="config"> The configuration. </param>
        public static void ExportLogFileOfFm(AcceptanceModelExportResultConfig config)
        {
            string diaPath = Path.Combine(config.WorkingDirectory,
                                          config.CurrentModelName,
                                          "dflowfm",
                                          "output",
                                          $"{config.CurrentModelName}.dia");

            if (!File.Exists(diaPath))
            {
                return;
            }

            FileUtils.CopyFile(diaPath,
                               Path.Combine(AcceptanceModelExportResultConfig.Delft3DfmExportDirectory, $"{config.OutputName}.dia"));

            config.HasExportedDiagnostics = true;
        }

        /// <summary>
        /// Export the log files of the different models inside an integrated model
        /// based up on the <paramref name="config"/>.
        /// </summary>
        /// <param name="config"> The configuration. </param>
        public static void ExportLogFilesOfIntegratedModel(AcceptanceModelExportResultConfig config)
        {
            string diaFolderFM = Path.Combine(config.WorkingDirectory,
                                              config.CurrentModelName,
                                              "dflowfm",
                                              "output");

            var directoryInfoFMOutput = new DirectoryInfo(diaFolderFM);
            if (directoryInfoFMOutput.Exists)
            {
                FileInfo[] diagFiles = directoryInfoFMOutput.GetFiles("*.dia");

                if (diagFiles.Any())
                {
                    FileUtils.CopyFile(diagFiles[0].FullName,
                                       Path.Combine(AcceptanceModelExportResultConfig.IntegratedModelsExportDirectory, $"{config.OutputName}.FM.{diagFiles[0].Name}"));
                    config.HasExportedDiagnostics = true;
                }
            }

            string diaRtcPath = Path.Combine(config.WorkingDirectory,
                                             config.CurrentModelName,
                                             "rtc",
                                             "output", "diag.xml");

            if (File.Exists(diaRtcPath))
            {
                FileUtils.CopyFile(diaRtcPath,
                                   Path.Combine(AcceptanceModelExportResultConfig.IntegratedModelsExportDirectory, $"{config.OutputName}.RTC.diag.xml"));
                config.HasExportedDiagnostics = true;
            }

            string diaWavesPath = Path.Combine(config.WorkingDirectory,
                                               config.CurrentModelName,
                                               "wave",
                                               "output", "swn-diag.Waves");

            if (File.Exists(diaWavesPath))
            {
                FileUtils.CopyFile(diaWavesPath,
                                   Path.Combine(AcceptanceModelExportResultConfig.IntegratedModelsExportDirectory, $"{config.OutputName}.Waves.swn-diag.Waves"));
                config.HasExportedDiagnostics = true;
            }
        }
    }
}