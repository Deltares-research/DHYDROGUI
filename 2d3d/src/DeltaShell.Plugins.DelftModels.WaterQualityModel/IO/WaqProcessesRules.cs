using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class WaqProcessesRules : NGHSFileBase
    {
        private const string waqCoefficientValidationsCvFileName = "DWAQ_allowed_values.csv";
        private const int numberOfColumns = 6;
        private const string commentDelimeter = "#";
        private const char separator = ',';

        private static readonly ILog Log = LogManager.GetLogger(typeof(WaqProcessesRules));

        /// <summary>
        /// Reads the validation CSV.
        /// </summary>
        /// <param name="directoryPath"> The directory path. </param>
        /// <returns> </returns>
        public IList<WaqProcessValidationRule> ReadValidationCsv(string directoryPath)
        {
            directoryPath = string.IsNullOrEmpty(directoryPath) ? string.Empty : directoryPath;
            string filePath = Path.Combine(directoryPath, waqCoefficientValidationsCvFileName);
            if (!File.Exists(filePath))
            {
                string errMssg = Resources
                    .WaqProcessesRules_ReadValidationCsv_File__0__not_found_in_the_path__1___No_validations_will_be_done_for_the_coefficients_;
                Log.ErrorFormat(errMssg, waqCoefficientValidationsCvFileName, filePath);
                return new List<WaqProcessValidationRule>();
            }

            OpenInputFile(filePath);
            var rules = new List<WaqProcessValidationRule>();
            try
            {
                string line = null;
                while ((line = GetNextLine()) != null)
                {
                    /* NGHSFileBase already skips the commented lines, but just in case. */
                    if (line.StartsWith(commentDelimeter))
                    {
                        continue;
                    }

                    string[] lineFields = line.Split(separator);
                    if (lineFields.Length != numberOfColumns)
                    {
                        Log.Warn(
                            $"Skipped line {line} due to incorrect number of columns (expected {numberOfColumns}, read {lineFields.Length}) from {filePath}.");
                        continue;
                    }

                    rules.Add(new WaqProcessValidationRule
                    {
                        ProcessName = lineFields[0],
                        ParameterName = lineFields[1],
                        MinValue = lineFields[2],
                        MaxValue = lineFields[3],
                        ValueType = GetProcessValueType(lineFields[4]),
                        Dependency = lineFields[5]
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
            string normValue = valueName.Replace(" ", "").ToLowerInvariant();
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
}