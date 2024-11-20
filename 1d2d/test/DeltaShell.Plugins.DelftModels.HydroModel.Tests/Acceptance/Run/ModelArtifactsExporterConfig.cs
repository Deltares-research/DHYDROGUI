namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    /// <summary>
    /// The <see cref="ModelArtifactsExporterConfig"/> holds all
    /// information required to export the dia files to the location
    /// where the Acceptance Dia/Log Report is generated.
    /// </summary>
    public class ModelArtifactsExporterConfig
    {
        /// <summary> The root folder in which we put our diagnostic report folder (should be the checkout folder). </summary>
        private const string checkoutFolder = @"..\..\";

        /// <summary> The path to the artifacts folder. </summary>
        public const string ArtifactsFolder = checkoutFolder + @"\DiagnosticReport\Model run artifacts";

        /// <summary>
        /// Initialize a new <see cref="ModelArtifactsExporterConfig"/> class.
        /// </summary>
        /// <param name="workingDirectory"> The DeltaShell working directory. </param>
        /// <param name="modelName"> The current model name. </param>
        /// <param name="outputFolderFolderName"> The output folder name to which the log files are exported. </param>
        public ModelArtifactsExporterConfig(string workingDirectory, string modelName, string outputFolderFolderName)
        {
            WorkingDirectory = workingDirectory;
            CurrentModelName = modelName;
            OutputFolderName = outputFolderFolderName;
        }

        /// <summary>
        /// Gets the path to the DeltaShell working directory.
        /// </summary>
        public string WorkingDirectory { get; }

        /// <summary>
        /// Gets the name of the current model.
        /// </summary>
        public string CurrentModelName { get; }

        /// <summary>
        /// Gets the name of the output folder to where the log files are exported.
        /// </summary>
        public string OutputFolderName { get; }
    }
}