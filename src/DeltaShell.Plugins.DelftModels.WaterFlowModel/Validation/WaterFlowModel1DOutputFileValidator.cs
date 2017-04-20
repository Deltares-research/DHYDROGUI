using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public class WaterFlowModel1DOutputFileValidator
    {
        public ValidationReport Validate(string path)
        {
            var validationIssues = new List<ValidationIssue>();

            if (!File.Exists(path))
            {
                validationIssues.Add(new ValidationIssue(path, ValidationSeverity.Error,
                    string.Format(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotExist, path)));
            }

            // initial validation settings
            var timeDimensionExists = false;
            var numTimeDependentVariables = 0;
            var numLocationIdVariables = 0;
            var timeVariableExists = false;
            
            NetCdfFile outputFile = null;

            try
            {
                outputFile = NetCdfFile.OpenExisting(path);

                timeDimensionExists = outputFile.GetAllDimensions()
                    .Any(d => outputFile.GetDimensionName(d) == WaterFlowModel1DOutputFileConstants.DimensionKeys.Time);

                var variables = outputFile.GetVariables();
                foreach (var netCdfVariable in variables) // Cycle through all variables
                {
                    var variableName = outputFile.GetVariableName(netCdfVariable);
                    var attributes = outputFile.GetAttributes(netCdfVariable);
                    
                    if (variableName == WaterFlowModel1DOutputFileConstants.VariableNames.Time) // in case of 'time' variable
                    {
                        timeVariableExists = true;
                        var unit = attributes.FirstOrDefault(a => a.Key == WaterFlowModel1DOutputFileConstants.AttributeKeys.Units).Value;

                        if (unit == null) // check if unit attribute exists
                        {
                            validationIssues.Add(new ValidationIssue(variableName, ValidationSeverity.Error,
                                Resources.WaterFlowModel1DOutputFileValidator_Validate_TimeVariableDoesNotContainUnitAttribute));

                            continue;
                        }

                        // check if unit attribute is of a valid format
                        var unitString = unit.ToString().Trim();
                        var dateTimeString = unitString.Replace(WaterFlowModel1DOutputFileConstants.TimeVariableUnitValuePrefix, "").Trim();
                        DateTime t0;

                        if (!unitString.StartsWith(WaterFlowModel1DOutputFileConstants.TimeVariableUnitValuePrefix) ||
                            !DateTime.TryParseExact(dateTimeString, WaterFlowModel1DOutputFileConstants.DateTimeFormat,
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out t0))
                        {
                            validationIssues.Add(new ValidationIssue(variableName, ValidationSeverity.Error,
                                string.Format(Resources.WaterFlowModel1DOutputFileValidator_Validate_TimeVariableDoesNotContainValidUnitInRequiredFormat,
                                    WaterFlowModel1DOutputFileConstants.TimeVariableUnitValuePrefix, WaterFlowModel1DOutputFileConstants.DateTimeFormat)));
                        }
                        continue;
                    }

                    // check if this is a location_id variable
                    if (attributes.Any(a => a.Key == WaterFlowModel1DOutputFileConstants.AttributeKeys.CfRole &&
                            a.Value.ToString() == WaterFlowModel1DOutputFileConstants.AttributeValues.CfRole))
                    {
                        numLocationIdVariables++;
                    }

                    // check if this is a time-dependent variable
                    if (outputFile.GetVariableDimensionNames(netCdfVariable).Any(n => n == WaterFlowModel1DOutputFileConstants.VariableNames.Time))
                    {
                        numTimeDependentVariables++;
                    }

                    // all other variables are optional
                }

            }
            catch (Exception ex)
            {
                // handle any NetCdfFile reading errors
                validationIssues.Add(new ValidationIssue(path, ValidationSeverity.Error,
                    Resources.WaterFlowModel1DOutputFileReader_ReadMetaData_ErrorReadingNetCdfFile, path + ex.Message));
            }
            finally
            {
                if(outputFile != null)
                    outputFile.Close();
            }

            return GenerateValidationReport(path, timeDimensionExists, validationIssues, timeVariableExists, numLocationIdVariables, numTimeDependentVariables);
        }

        private ValidationReport GenerateValidationReport(string path, bool timeDimensionExists, List<ValidationIssue> validationIssues,
                                                          bool timeVariableExists, int numLocationIdVariables, int numTimeDependentVariables)
        {
            if (!timeDimensionExists) // time dimension doesn't exist
            {
                validationIssues.Add(new ValidationIssue(path, ValidationSeverity.Error,
                    string.Format(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotContainRequiredTimeDimension, path)));
            }

            if (!timeVariableExists) // time variable doesn't exist
            {
                validationIssues.Add(new ValidationIssue(path, ValidationSeverity.Error,
                    string.Format(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotContainRequiredTimeVariable, path)));
            }

            if (numLocationIdVariables < 1) // locationId variable does not exist
            {
                validationIssues.Add(new ValidationIssue(path, ValidationSeverity.Error,
                    string.Format(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotContainRequiredLocationIdVariable, path)));
            }

            if (numLocationIdVariables > 1) // more than 1 locationId variables exist
            {
                validationIssues.Add(new ValidationIssue(path, ValidationSeverity.Error,
                    string.Format(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileContainsMoreThan1LocationIdVariable, path)));
            }

            if (numTimeDependentVariables == 0) // no time-dependent variables exist
            {
                validationIssues.Add(new ValidationIssue(path, ValidationSeverity.Warning,
                    string.Format(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotContainAnyTimeDependentVariables, path)));
            }

            return new ValidationReport(this.GetType().Name, validationIssues);
        }

    }
}
