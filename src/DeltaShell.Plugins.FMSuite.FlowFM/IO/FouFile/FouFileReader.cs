using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Provides a reader for statistical analysis input files (*.fou).
    /// </summary>
    public sealed class FouFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FouFileReader));

        private readonly WaterFlowFMModelDefinition modelDefinition;
        private readonly FouFileDefinition fouFileDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="FouFileReader"/> class.
        /// </summary>
        /// <param name="modelDefinition">The model definition that defines which fou variables are enabled.</param>
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

        private static string ReadFromFile(string path)
        {
            log.Info($"Reading statistical analysis input file from '{path}'.");

            return File.ReadAllText(path);
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

        private static IEnumerable<FouFileVariable> ParseVariables(string contents)
        {
            var variables = new List<FouFileVariable>();
            var stringReader = new StringReader(contents);

            stringReader.ReadLine(); // skip header line

            string line;

            while ((line = stringReader.ReadLine()) != null)
            {
                if (TryParseVariable(line, out FouFileVariable variable))
                {
                    variables.Add(variable);
                }
            }

            return variables;
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
                    Name = tokens[0],
                    StartTime = ConvertValueFromString<double>(tokens[1], nameof(FouFileVariable.StartTime)),
                    StopTime = ConvertValueFromString<double>(tokens[2], nameof(FouFileVariable.StopTime)),
                    NumberOfCycles = ConvertValueFromString<int>(tokens[3], nameof(FouFileVariable.NumberOfCycles)),
                    AmplificationFactor = ConvertValueFromString<int>(tokens[4], nameof(FouFileVariable.AmplificationFactor)),
                    AstronomicalArgument = ConvertValueFromString<int>(tokens[5], nameof(FouFileVariable.AstronomicalArgument)),
                };

                if (tokens.Length == 7)
                {
                    try
                    {
                        variable.LayerNumber = ConvertValueFromString<int>(tokens[6], nameof(FouFileVariable.LayerNumber));
                    }
                    catch
                    {
                        variable.EllipticParameters = tokens[6];
                    }
                }
                else if (tokens.Length == 8)
                {
                    variable.LayerNumber = ConvertValueFromString<int>(tokens[6], nameof(FouFileVariable.LayerNumber));
                    variable.EllipticParameters = tokens[7];
                }

                return true;
            }
            catch (FormatException e)
            {
                log.Error($"Failed parsing fou file line: '{line}'.", e);

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

            IEnumerable<string> propertiesToDisable = supportedVariables.Select(GetPropertyName).Where(name => name != null);
            IEnumerable<string> propertiesToEnable = parsedVariables.Select(GetPropertyName).Where(name => name != null);

            propertiesToDisable.ToList().ForEach(name => SetPropertyValue(name, false));
            propertiesToEnable.ToList().ForEach(name => SetPropertyValue(name, true));

            SetPropertyValue(GuiProperties.WriteFouFile, true);
        }

        private string GetPropertyName(FouFileVariable variable)
        {
            string propertyName = fouFileDefinition.GetModelPropertyName(variable);

            if (string.IsNullOrEmpty(propertyName))
            {
                log.Error($"Cannot find model property for fou file variable: '{variable.Name}'.");
            }

            return propertyName;
        }

        private void SetPropertyValue<T>(string propertyName, T propertyValue)
        {
            WaterFlowFMProperty writeFouFileProperty = modelDefinition.GetModelProperty(propertyName);
            writeFouFileProperty.Value = propertyValue;
        }
    }
}