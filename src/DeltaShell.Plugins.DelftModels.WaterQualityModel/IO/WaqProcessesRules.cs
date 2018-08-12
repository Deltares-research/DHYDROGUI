using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class WaqProcessesRules: NGHSFileBase
    {
        const string waqCoefficientValidationsCvFileName = "DWAQ_allowed_values.csv";
        private const int numberOfColumns = 6;
        private const string commentDelimeter = "#";
        private const char separator = ',';

        private static readonly ILog Log = LogManager.GetLogger(typeof(WaqProcessesRules));

        /// <summary>
        /// Reads the validation CSV.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns></returns>
        public IList<WaqProcessValidationRule> ReadValidationCsv(string directoryPath)
        {
            var filePath = Path.Combine(directoryPath, waqCoefficientValidationsCvFileName);
            OpenInputFile(filePath);
            var rules = new List<WaqProcessValidationRule>();
            try
            {
                string line = null;
                while((line = GetNextLine()) != null)
                {
                    /* NGHSFileBase already skips the commented lines, but just in case. */
                    if (line.StartsWith(commentDelimeter)) continue;

                    var lineFields = line.Split(separator);
                    if (lineFields.Length != numberOfColumns)
                    {
                        Log.Warn($"Skipped line {line} due to incorrect number of columns (expected {numberOfColumns}, read {lineFields.Length}) from {filePath}.");
                        continue;
                    }

                    rules.Add(new WaqProcessValidationRule
                    {
                        ProcessName = lineFields[0],
                        ParameterName = lineFields[1],
                        MinValue = lineFields[2],
                        MaxValue = lineFields[3],
                        ValueType = GetProcessValueType(lineFields[4]),
                        Dependency = lineFields[5],
                    });
                }
            }
            finally
            {
                CloseInputFile();
            }

            return rules;
        }

        private static Type GetProcessValueType(string valueName)
        {
            var normValue = valueName.Replace(" ", "").ToLowerInvariant();
            switch (normValue)
            {
                case "int":
                    return typeof(int);
                case "double":
                    return typeof(double);
                default:
                    return typeof(double);
            }
        }
    }

    /// <summary>
    /// Class that represents a rule
    /// </summary>
    public class WaqProcessValidationRule
    {
        /// <summary>
        /// Gets or sets the name of the process.
        /// </summary>
        /// <value>
        /// The name of the process.
        /// </value>
        public string ProcessName { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// <value>
        /// The name of the parameter.
        /// </value>
        public string ParameterName { get; set; }
        
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        public string MinValue { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public string MaxValue { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the value.
        /// </summary>
        /// <value>
        /// The type of the value.
        /// </value>
        public Type ValueType { get; set; }
        
        /// <summary>
        /// Gets or sets the dependency.
        /// </summary>
        /// <value>
        /// The dependency.
        /// </value>
        public string Dependency { get; set; }
    }
}