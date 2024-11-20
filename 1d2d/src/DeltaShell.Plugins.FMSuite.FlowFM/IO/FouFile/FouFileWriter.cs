using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Provides a writer for the statistical analysis configuration file (*.fou).
    /// </summary>
    /// <remarks>
    /// The *.fou file is an input configuration file that determines the online statistical analysis
    /// to be performed on model output quantities. The analysis parameters defined in the file can
    /// include the time period, the number of cycles, and the frequency range for the analysis.
    /// While a Fourier analysis can be applied, the statistical methods are not limited to Fourier;
    /// other forms of analysis, such as computing minimum, maximum, or average values, are also supported.
    /// <para/>
    /// The results of the statistical analysis, based on the quantities defined in the *.fou file,
    /// are written to an output file (e.g., *_fou.nc).
    /// </remarks>
    public sealed class FouFileWriter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FouFileWriter));

        private readonly WaterFlowFMModelDefinition modelDefinition;
        private readonly FouFileDefinition fouFileDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="FouFileWriter"/> class.
        /// </summary>
        /// <param name="modelDefinition">The model definition that defines which quantities are enabled for statistical analysis.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="modelDefinition"/> is <c>null</c>.</exception>
        public FouFileWriter(WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));

            this.modelDefinition = modelDefinition;
            fouFileDefinition = new FouFileDefinition();
        }

        /// <summary>
        /// Returns whether fou file writing is enabled.
        /// </summary>
        /// <returns><c>true</c> when file writing is enabled; otherwise <c>false</c>.</returns>
        public bool CanWrite()
        {
            WaterFlowFMProperty writeFouFileProperty = modelDefinition.GetModelProperty(GuiProperties.WriteFouFile);
            return (bool)writeFouFileProperty.Value;
        }

        /// <summary>
        /// Writes the fou file to the specified directory.
        /// </summary>
        /// <param name="directory">The directory to which to write the fou file.</param>
        /// <exception cref="ArgumentException">When <paramref name="directory"/> is <c>null</c> or empty.</exception>
        /// <exception cref="InvalidOperationException">When fou file writing is disabled in the model definition.</exception>
        public void WriteToDirectory(string directory)
        {
            Ensure.NotNullOrEmpty(directory, nameof(directory));

            if (!CanWrite())
            {
                throw new InvalidOperationException("fou file writing is disabled in the model definition.");
            }

            string path = GetFilePath(directory);
            string contents = GetFileContents();

            WriteToFile(path, contents);
        }

        private static void WriteToFile(string path, string contents)
        {
            log.InfoFormat(Resources.Writing_statistical_analysis_input_file_to___0___, path);

            File.WriteAllText(path, contents);
        }

        private string GetFilePath(string directory)
        {
            string fileName = GetOrUpdateFileName();
            return Path.Combine(directory, fileName);
        }

        private string GetOrUpdateFileName()
        {
            WaterFlowFMProperty fouFileProperty = modelDefinition.GetModelProperty(KnownProperties.FouFile);

            if (string.IsNullOrEmpty((string)fouFileProperty.Value))
            {
                fouFileProperty.Value = FouFileConstants.DefaultFileName;
            }

            return (string)fouFileProperty.Value;
        }

        private string GetFileContents()
        {
            var stringBuilder = new StringBuilder();

            AppendHeader(stringBuilder);
            AppendVariables(stringBuilder);

            return stringBuilder.ToString();
        }

        private static void AppendHeader(StringBuilder stringBuilder)
        {
            stringBuilder.Append(FouFileConstants.FileHeader);
        }

        private void AppendVariables(StringBuilder stringBuilder)
        {
            IEnumerable<FouFileVariable> variables = fouFileDefinition.Variables;
            IEnumerable<FouFileVariable> variablesToWrite = variables.Where(CanWriteVariable);

            foreach (FouFileVariable variable in variablesToWrite)
            {
                SetStartStopTime(variable);
                AppendVariable(stringBuilder, variable);
            }
        }

        private bool CanWriteVariable(FouFileVariable variable)
        {
            string modelPropertyName = fouFileDefinition.GetModelPropertyName(variable);
            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(modelPropertyName);

            if (modelProperty == null)
            {
                log.ErrorFormat(Resources.No_D_Flow_FM_property_defined_for_fou_file_quantity____0___with_analysis_type____1___, variable.Quantity, variable.AnalysisType);
                return false;
            }

            return (bool)modelProperty.Value;
        }

        private void SetStartStopTime(FouFileVariable variable)
        {
            WaterFlowFMProperty startTime = modelDefinition.GetModelProperty(KnownProperties.TStart);
            WaterFlowFMProperty stopTime = modelDefinition.GetModelProperty(KnownProperties.TStop);

            variable.StartTime = (double)startTime.Value;
            variable.StopTime = (double)stopTime.Value;
        }

        private static void AppendVariable(StringBuilder stringBuilder, FouFileVariable variable)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append($"{variable.Quantity,-FouFileConstants.ColumnWidth}");
            stringBuilder.Append($"{variable.StartTime,-FouFileConstants.ColumnWidth}");
            stringBuilder.Append($"{variable.StopTime,-FouFileConstants.ColumnWidth}");
            stringBuilder.Append($"{variable.NumberOfCycles,-FouFileConstants.ColumnWidth}");
            stringBuilder.Append($"{variable.AmplificationFactor,-FouFileConstants.ColumnWidth}");
            stringBuilder.Append($"{variable.PhaseShift,-FouFileConstants.ColumnWidth}");
            stringBuilder.Append($"{variable.LayerNumber,-FouFileConstants.ColumnWidth}");
            stringBuilder.Append(variable.AnalysisType);
        }
    }
}