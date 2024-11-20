using System.IO;
using System.Linq;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    /// <summary>
    /// The AcceptanceModelExportHelper provides functionality for exporting
    /// the dia and log files used within TeamCity to generate reports.
    /// </summary>
    public sealed class ModelArtifactsExporter
    {
        private bool hasExportedLogFiles;
        private readonly ModelArtifactsExporterConfig config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelArtifactsExporter"/> class.
        /// </summary>
        /// <param name="config"> The configuration for the exporter. </param>
        public ModelArtifactsExporter(ModelArtifactsExporterConfig config)
        {
            this.config = config;
        }

        /// <summary>
        /// Export the log files of the model.
        /// The model can be standalone and integrated.
        /// </summary>
        public void ExportModelLogFiles()
        {
            string exportFolder = Path.Combine(ModelArtifactsExporterConfig.ArtifactsFolder, config.OutputFolderName);
            FileUtils.CreateDirectoryIfNotExists(exportFolder, true);

            ExportFMLogFile(exportFolder);
            ExportRRLogFile(exportFolder);
            ExportRtcLogFile(exportFolder);
            ExportEmptyLogFile(exportFolder);
        }

        private void ExportFMLogFile(string modelExportSubFolder)
        {
            string diaFolderFM = Path.Combine(config.WorkingDirectory,
                                              config.CurrentModelName,
                                              "dflowfm",
                                              "output");

            var directoryInfoFMOutput = new DirectoryInfo(diaFolderFM);
            if (!directoryInfoFMOutput.Exists)
            {
                return;
            }

            FileInfo[] diagFiles = directoryInfoFMOutput.GetFiles("*.dia");

            if (diagFiles.Any())
            {
                FileUtils.CopyFile(diagFiles[0].FullName,
                                   Path.Combine(modelExportSubFolder, diagFiles[0].Name));
                hasExportedLogFiles = true;
            }
        }

        private void ExportRRLogFile(string modelExportSubFolder)
        {
            string diaRRPath = Path.Combine(config.WorkingDirectory,
                                            config.CurrentModelName,
                                            "rr",
                                            "sobek_3b.log");

            if (!File.Exists(diaRRPath))
            {
                return;
            }

            FileUtils.CopyFile(diaRRPath,
                               Path.Combine(modelExportSubFolder, "sobek_3b.log"));
            hasExportedLogFiles = true;
        }

        private void ExportRtcLogFile(string modelExportSubFolder)
        {
            string diaRtcPath = Path.Combine(config.WorkingDirectory,
                                             config.CurrentModelName,
                                             "rtc",
                                             "output", "diag.xml");

            if (!File.Exists(diaRtcPath))
            {
                return;
            }

            FileUtils.CopyFile(diaRtcPath,
                               Path.Combine(modelExportSubFolder, "diag.xml"));
            hasExportedLogFiles = true;
        }

        private void ExportEmptyLogFile(string folder)
        {
            if (hasExportedLogFiles)
            {
                return;
            }

            const string content = "No diagnostics files were produced.";

            File.WriteAllText(Path.Combine(folder, $"Empty"), content);
        }
    }
}