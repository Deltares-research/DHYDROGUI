using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Provides a writer for statistical analysis input files (*.fou).
    /// </summary>
    public sealed class FouFileWriter
    {
        public const string DefaultFileName = "Maxima.fou";

        private const int columnWidth = 10;

        private static readonly ILog log = LogManager.GetLogger(typeof(FouFileWriter));

        private readonly WaterFlowFMModelDefinition modelDefinition;
        private readonly FouFileDefinition fouFileDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="FouFileWriter"/> class.
        /// </summary>
        /// <param name="modelDefinition">The model definition that defines which fou variables are enabled.</param>
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
                throw new InvalidOperationException("Fou file writing is disabled in the model definition.");
            }

            string path = GetFilePath(directory);
            string contents = GetFileContents();

            WriteToFile(path, contents);
        }

        private void WriteToFile(string path, string contents)
        {
            log.Info($"Writing statistical analysis input file to '{path}'.");

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
                fouFileProperty.Value = DefaultFileName;
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

        private void AppendHeader(StringBuilder stringBuilder)
        {
            var columnTitles = new[]
            {
                "*var",
                "tsrts",
                "sstop",
                "numcyc",
                "knfac",
                "v0plu",
                "layno",
                "elp"
            };

            foreach (string title in columnTitles)
            {
                stringBuilder.Append($"{title,-columnWidth}");
            }
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
            return (bool)modelProperty.Value;
        }

        private void SetStartStopTime(FouFileVariable variable)
        {
            WaterFlowFMProperty startTime = modelDefinition.GetModelProperty(KnownProperties.TStart);
            WaterFlowFMProperty stopTime = modelDefinition.GetModelProperty(KnownProperties.TStop);

            variable.StartTime = (double)startTime.Value;
            variable.StopTime = (double)stopTime.Value;
        }

        private void AppendVariable(StringBuilder stringBuilder, FouFileVariable variable)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append($"{variable.Name,-columnWidth}");
            stringBuilder.Append($"{variable.StartTime,-columnWidth}");
            stringBuilder.Append($"{variable.StopTime,-columnWidth}");
            stringBuilder.Append($"{variable.NumberOfCycles,-columnWidth}");
            stringBuilder.Append($"{variable.AmplificationFactor,-columnWidth}");
            stringBuilder.Append($"{variable.AstronomicalArgument,-columnWidth}");
            stringBuilder.Append($"{variable.LayerNumber,-columnWidth}");
            stringBuilder.Append(variable.EllipticParameters);
        }
    }
}