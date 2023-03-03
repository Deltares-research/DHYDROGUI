using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.NetCdf;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    public static class NetcdfFileValidator
    {
        /// <summary>
        /// Validates if a netcdf file has the same dimensions and variables as a reference netcdf file.
        /// </summary>
        /// <param name="actualFile">The actual netcdf file.</param>
        /// <param name="expectedFile">The expected netcdf file.</param>
        /// <returns>Returns any validation issues.</returns>
        public static ICollection<string> Validate(string actualFile, string expectedFile)
        {
            string fileName = Path.GetFileName(actualFile);
            
            NetCdfFile actualNetCdfFile = NetCdfFile.OpenExisting(actualFile, false);
            NetCdfFile expectedNetCdfFile = NetCdfFile.OpenExisting(expectedFile, false);

            try
            {
                IEnumerable<string> dimensionValidationIssues = ValidateDimensions(actualNetCdfFile, expectedNetCdfFile, fileName);
                IEnumerable<string> variableValidationIssues = ValidateVariables(actualNetCdfFile, expectedNetCdfFile, fileName);

                return dimensionValidationIssues.Concat(variableValidationIssues).ToArray();
            }
            catch (Exception e)
            {
                return new[]
                {
                    $"An exception occurred while comparing the output file {fileName}: {e.Message}"
                };
            }
            finally
            {
                actualNetCdfFile.Close();
                expectedNetCdfFile.Close();
            }
        }
        
        private static IEnumerable<string> ValidateDimensions(NetCdfFile actualNetCdfFile, NetCdfFile expectedNetCdfFile, string fileName)
        {
            NetCdfDimension[] actualDimensions = actualNetCdfFile.GetAllDimensions().ToArray();
            NetCdfDimension[] expectedDimensions = expectedNetCdfFile.GetAllDimensions().ToArray();

            Dictionary<string, int> actualDimensionLookup = actualDimensions.ToDictionary(actualNetCdfFile.GetDimensionName, 
                                                                                          actualNetCdfFile.GetDimensionLength, 
                                                                                          StringComparer.InvariantCultureIgnoreCase);
            Dictionary<string, int> expectedDimensionLookup = expectedDimensions.ToDictionary(expectedNetCdfFile.GetDimensionName, 
                                                                                              expectedNetCdfFile.GetDimensionLength, 
                                                                                              StringComparer.InvariantCultureIgnoreCase);

            foreach (KeyValuePair<string,int> actualDimensionKeyValuePair in actualDimensionLookup)
            {
                string actualDimensionName = actualDimensionKeyValuePair.Key;
                if (expectedDimensionLookup.TryGetValue(actualDimensionName, out int expectedDimensionLength))
                {
                    int actualDimensionLength = actualDimensionKeyValuePair.Value;
                    
                    if (expectedDimensionLength != actualDimensionLength)
                    {
                        yield return $"The actual netcdf file '{fileName}' contains a dimension '{actualDimensionName}' with a value of '{actualDimensionLength}', but '{expectedDimensionLength}' was expected.";
                    }
                }
            }

            IEnumerable<string> missingDimensions = expectedDimensionLookup.Keys.Except(actualDimensionLookup.Keys).ToArray();
            if (missingDimensions.Any())
            {
                yield return $"The actual netcdf file '{fileName}' is missing the following dimension(s): '{string.Join(Environment.NewLine, missingDimensions)}'";
            }
        }
        
        private static IEnumerable<string> ValidateVariables(NetCdfFile actualNetCdfFile, NetCdfFile expectedNetCdfFile, string fileName)
        {
            IEnumerable<NetCdfVariable> actualVariables = actualNetCdfFile.GetVariables();
            IEnumerable<NetCdfVariable> expectedVariables = expectedNetCdfFile.GetVariables();

            Dictionary<string, NetCdfVariable> actualVariableLookup = actualVariables
                .ToDictionary(actualNetCdfFile.GetVariableName, StringComparer.InvariantCultureIgnoreCase);

            Dictionary<string, NetCdfVariable> expectedVariableLookup = expectedVariables
                .ToDictionary(expectedNetCdfFile.GetVariableName, StringComparer.InvariantCultureIgnoreCase);

            foreach (KeyValuePair<string, NetCdfVariable> actualVariableKeyValuePair in actualVariableLookup)
            {
                string actualVariableName = actualVariableKeyValuePair.Key;
                if (expectedVariableLookup.TryGetValue(actualVariableName, out NetCdfVariable expectedVariable))
                {
                    NetCdfVariable actualVariable = actualVariableKeyValuePair.Value;

                    IEnumerable<string> dimensionValidationIssues = ValidateVariableDimensionNames(actualNetCdfFile, expectedNetCdfFile, expectedVariable, actualVariable, fileName, actualVariableName);
                    foreach (string dimensionValidationIssue in dimensionValidationIssues)
                    {
                        yield return dimensionValidationIssue;
                    }
                    
                    Dictionary<string, object> expectedAttributes = expectedNetCdfFile.GetAttributes(expectedVariable);
                    Dictionary<string, object> actualAttributes = actualNetCdfFile.GetAttributes(actualVariable);
                    IEnumerable<string> attributeValidationIssues = ValidateVariableAttributes(actualAttributes, expectedAttributes, fileName);
                    foreach (string attributeValidationIssue in attributeValidationIssues)
                    {
                        yield return attributeValidationIssue;
                    }
                }
            }

            IEnumerable<string> missingVariables = expectedVariableLookup.Keys.Except(actualVariableLookup.Keys).ToArray();
            if (missingVariables.Any())
            {
                yield return $"The actual netcdf file '{fileName}' is missing the following variable(s): '{string.Join(Environment.NewLine, missingVariables)}'";
            }
        }

        private static IEnumerable<string> ValidateVariableAttributes(Dictionary<string, object> actualAttributes, 
                                                                      Dictionary<string, object> expectedAttributes, 
                                                                      string fileName)
        {
            IEnumerable<string> missingAttributes = expectedAttributes.Keys.Except(actualAttributes.Keys).ToArray();
            if (missingAttributes.Any())
            {
                yield return $"The actual netcdf file '{fileName}' is missing the following attribute(s): {string.Join(Environment.NewLine, missingAttributes)}";
            }
        }

        private static IEnumerable<string> ValidateVariableDimensionNames(NetCdfFile actualNetCdfFile, NetCdfFile expectedNetCdfFile, 
                                                                          NetCdfVariable expectedVariable, NetCdfVariable actualVariable,
                                                                          string fileName, string variableName)
        {
            IEnumerable<string> expectedDimensionNames = GetVariableDimensionNames(expectedVariable, expectedNetCdfFile);
            IEnumerable<string> actualDimensionNames = GetVariableDimensionNames(actualVariable, actualNetCdfFile);
            IEnumerable<string> missingDimensions = expectedDimensionNames.Except(actualDimensionNames).ToArray();

            if (missingDimensions.Any())
            {
                yield return $"The actual netcdf file '{fileName}' contains a variable '{variableName}' that is missing the following dimension(s): {string.Join(Environment.NewLine, missingDimensions)}";
            }
        }

        private static string[] GetVariableDimensionNames(NetCdfVariable variable, NetCdfFile file)
        {
            return file.GetDimensions(variable).Select(file.GetDimensionName).ToArray();
        }
    }
}