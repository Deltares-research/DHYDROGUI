using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Provides a reader for the statistical analysis configuration file (*.fou).
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
    public sealed class FouFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FouFileReader));

        private readonly WaterFlowFMModelDefinition modelDefinition;
        private readonly FouFileDefinition fouFileDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="FouFileReader"/> class.
        /// </summary>
        /// <param name="modelDefinition">The model definition that defines which quantities are enabled for statistical analysis.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="modelDefinition"/> is <c>null</c>.</exception>
        public FouFileReader(WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));

            this.modelDefinition = modelDefinition;
            fouFileDefinition = new FouFileDefinition();
        }

        /// <summary>
        /// Returns whether the fou file can be read from the specified directory.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <returns><c>true</c> when file writing is enabled; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentException">When <paramref name="directory"/> is <c>null</c> or empty.</exception>
        public bool CanReadFromDirectory(string directory)
        {
            Ensure.NotNullOrEmpty(directory, nameof(directory));

            string fileName = GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            string filePath = GetFilePath(directory);
            if (!File.Exists(filePath))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads the fou file from the specified directory.
        /// </summary>
        /// <param name="directory">The directory from which to read the fou file.</param>
        /// <exception cref="ArgumentException">When <paramref name="directory"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentException">When <paramref name="directory"/> does not contain a fou file.</exception>
        public void ReadFromDirectory(string directory)
        {
            Ensure.NotNullOrEmpty(directory, nameof(directory));

            if (!CanReadFromDirectory(directory))
            {
                throw new ArgumentException(@"Configured fou file not found in directory.", nameof(directory));
            }

            string path = GetFilePath(directory);
            string contents = ReadFromFile(path);

            IEnumerable<FouFileVariable> variables = ParseVariables(contents);
            UpdateModelDefinition(variables);
        }

        private string GetFilePath(string directory)
        {
            string fileName = GetFileName();
            return Path.Combine(directory, fileName);
        }

        private string GetFileName()
        {
            WaterFlowFMProperty fouFileProperty = modelDefinition.GetModelProperty(KnownProperties.FouFile);
            return (string)fouFileProperty.Value;
        }

        private static string ReadFromFile(string path)
        {
            log.InfoFormat(Resources.Reading_statistical_analysis_input_file_from___0___, path);

            return File.ReadAllText(path);
        }

        private static IEnumerable<FouFileVariable> ParseVariables(string contents)
        {
            var variables = new List<FouFileVariable>();
            var stringReader = new StringReader(contents);

            string line;

            while ((line = stringReader.ReadLine()) != null)
            {
                line = CleanLine(line);

                if (IsCommentLine(line))
                {
                    continue;
                }

                if (TryParseVariable(line, out FouFileVariable variable))
                {
                    variables.Add(variable);
                }
            }

            return variables;
        }

        private static string CleanLine(string line)
        {
            line = line.Replace('\t', ' ');
            line = line.Trim();

            return line;
        }

        private static bool IsCommentLine(string line)
        {
            return line.StartsWith(FouFileConstants.CommentDelimiter);
        }

        private static bool TryParseVariable(string line, out FouFileVariable variable)
        {
            string[] tokens = line.SplitOnEmptySpace();

            if (tokens.Length < 6 || tokens.Length > 8)
            {
                variable = null;
                return false;
            }

            try
            {
                variable = new FouFileVariable
                {
                    Quantity = tokens[0].ToLower(),
                    StartTime = ConvertValueFromString<double>(tokens[1], nameof(FouFileVariable.StartTime)),
                    StopTime = ConvertValueFromString<double>(tokens[2], nameof(FouFileVariable.StopTime)),
                    NumberOfCycles = ConvertValueFromString<int>(tokens[3], nameof(FouFileVariable.NumberOfCycles)),
                    AmplificationFactor = ConvertValueFromString<int>(tokens[4], nameof(FouFileVariable.AmplificationFactor)),
                    PhaseShift = ConvertValueFromString<int>(tokens[5], nameof(FouFileVariable.PhaseShift)),
                };

                if (tokens.Length == 7)
                {
                    try
                    {
                        variable.LayerNumber = ConvertValueFromString<int>(tokens[6], nameof(FouFileVariable.LayerNumber));
                    }
                    catch
                    {
                        variable.AnalysisType = tokens[6].ToLower();
                    }
                }
                else if (tokens.Length == 8)
                {
                    variable.LayerNumber = ConvertValueFromString<int>(tokens[6], nameof(FouFileVariable.LayerNumber));
                    variable.AnalysisType = tokens[7].ToLower();
                }

                return true;
            }
            catch (FormatException e)
            {
                log.Error(string.Format(Resources.Failed_parsing_fou_file_line____0___, line), e);

                variable = null;
                return false;
            }
        }

        private static T ConvertValueFromString<T>(string token, string tokenName) where T : struct, IConvertible
        {
            try
            {
                double parsedValue = double.Parse(token, NumberFormatInfo.InvariantInfo);
                return (T)Convert.ChangeType(parsedValue, typeof(T), NumberFormatInfo.InvariantInfo);
            }
            catch (Exception e) when (e is FormatException || e is InvalidCastException)
            {
                throw new FormatException(
                    $"Token {tokenName} has formatted string value '{token}' which could not be parsed as {typeof(T)}", e);
            }
        }

        private void UpdateModelDefinition(IEnumerable<FouFileVariable> parsedVariables)
        {
            IEnumerable<FouFileVariable> supportedVariables = fouFileDefinition.Variables;

            supportedVariables.Select(GetModelPropertyName).ForEach(name => SetModelPropertyValue(name, false));
            parsedVariables.Where(HasSupportedQuantity).Select(GetModelPropertyName).ForEach(name => SetModelPropertyValue(name, true));

            SetModelPropertyValue(GuiProperties.WriteFouFile, true);
        }

        private bool HasSupportedQuantity(FouFileVariable variable)
        {
            if (FouFileQuantities.IsUnsupportedQuantity(variable.Quantity))
            {
                log.WarnFormat(Resources.The_selected_fou_file_quantity___0___is_not_yet_available_or_validated_for_1D_, variable.Quantity);
                return false;
            }

            if (modelDefinition.GetModelProperty(GetModelPropertyName(variable)) == null)
            {
                log.ErrorFormat(Resources.No_D_Flow_FM_property_defined_for_fou_file_quantity____0___with_analysis_type____1___, variable.Quantity, variable.AnalysisType);
                return false;
            }

            return true;
        }

        private string GetModelPropertyName(FouFileVariable variable)
        {
            return fouFileDefinition.GetModelPropertyName(variable);
        }

        private void SetModelPropertyValue<T>(string propertyName, T propertyValue)
        {
            WaterFlowFMProperty writeFouFileProperty = modelDefinition.GetModelProperty(propertyName);
            writeFouFileProperty.Value = propertyValue;
        }
    }
}