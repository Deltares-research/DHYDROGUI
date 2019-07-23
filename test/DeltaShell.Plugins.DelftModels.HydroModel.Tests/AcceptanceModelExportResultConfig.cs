namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    /// <summary>
    /// The <see cref="AcceptanceModelExportResultConfig"/> holds all
    /// information required to export the dia files to the location
    /// where the Acceptance Dia/Log Report is generated.
    /// </summary>
    public class AcceptanceModelExportResultConfig
    {
        /// <summary>
        /// Initialize a new <see cref="AcceptanceModelExportResultConfig"/> with default empty values.
        /// </summary>
        public AcceptanceModelExportResultConfig()
        {
            WorkingDirectory = string.Empty;
            CurrentModelName = string.Empty;
            OutputName = string.Empty;
            HasExportedDiagnostics = false;
            HasValidated = false;
        }

        /// <summary> The root folder in which we put our diagnostic report folder (should be the checkout folder). </summary>
        public const string ReleaseFolder = @"..\..\";

        /// <summary> The path to the report folder. </summary>
        public const string ReportFolder = ReleaseFolder + @"\DiagnosticReport";

        /// <summary> The delft3dfm export directory. </summary>
        public const string Delft3DfmExportDirectory = ReportFolder + @"\DELFT3D-FM";

        /// <summary>
        /// Gets or sets the path to the working directory as used by the tests.
        /// </summary>
        /// <value> The path to the working directory. </value>
        public string WorkingDirectory { get; set; }
        /// <summary>
        /// Gets or sets the name of the current model.
        /// </summary>
        /// <value>The name of the current model.</value>
        public string CurrentModelName { get; set; }
        /// <summary>
        /// Gets or sets the name of the output.
        /// </summary>
        /// <value>The name of the output.</value>
        public string OutputName { get; set; }
        /// <summary>
        /// Gets or sets whether the current test has exported its diagnostic file.
        /// </summary>
        /// <value><c>true</c> if this test run has exported its diagnostic file; otherwise, <c>false</c>.</value>
        public bool HasExportedDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets whether this test run has successfully been validated.
        /// </summary>
        /// <value><c>true</c> if this test run has successfully been validated; otherwise, <c>false</c>.</value>
        public bool HasValidated { get; set; }
    }
}
