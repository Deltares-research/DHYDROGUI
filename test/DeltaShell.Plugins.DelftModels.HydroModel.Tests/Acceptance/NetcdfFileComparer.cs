using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.NetCdf;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    public static class NetcdfFileComparer
    {
        /// <summary>
        /// Compares if a netcdf file has the same dimensions and variables as a reference netcdf file.
        /// </summary>
        /// <param name="actualFile">The actual netcdf file.</param>
        /// <param name="expectedFile">The expected netcdf file.</param>
        /// <param name="overallErrorMessage">Reference to an overall error messages.</param>
        /// <returns><c>True</c> if both files have the same dimensions and variables.</returns>
        public static bool Compare(string actualFile, string expectedFile, ref string overallErrorMessage)
        {
            string fileName = Path.GetFileName(actualFile);
            
            bool sameDimensions = true;
            bool sameVariables = true;
            
            NetCdfFile actualNetCdfFile = NetCdfFile.OpenExisting(actualFile, false);
            NetCdfFile expectedNetCdfFile = NetCdfFile.OpenExisting(expectedFile, false);

            try
            {
                sameDimensions = CompareDimensions(actualNetCdfFile, expectedNetCdfFile, fileName, ref overallErrorMessage);
                sameVariables = CompareVariables(actualNetCdfFile, expectedNetCdfFile, fileName, ref overallErrorMessage);
            }
            catch (Exception e)
            {
                overallErrorMessage += $"An exception occurred while comparing the output file {fileName}: {e.Message}";
                return false;
            }
            finally
            {
                actualNetCdfFile.Close();
                expectedNetCdfFile.Close();
            }

            return sameDimensions && sameVariables;
        }
        
        
        private static bool CompareDimensions(NetCdfFile actualNetCdfFile,
                                              NetCdfFile expectedNetCdfFile,
                                              string fileName,
                                              ref string overallErrorMessage)
        {
            IEnumerable<NetCdfDimension> actualDimensions = actualNetCdfFile.GetAllDimensions();
            IEnumerable<NetCdfDimension> expectedDimensions = expectedNetCdfFile.GetAllDimensions();

            bool dimensionsAreSame = VerifyActualDimensions(actualDimensions,
                                                            expectedDimensions,
                                                            actualNetCdfFile,
                                                            expectedNetCdfFile,
                                                            fileName,
                                                            ref overallErrorMessage);
            bool hasMissingDimensions = CheckForMissingDimensions(actualDimensions,
                                                                  expectedDimensions,
                                                                  actualNetCdfFile,
                                                                  expectedNetCdfFile,
                                                                  fileName,
                                                                  ref overallErrorMessage);

            return dimensionsAreSame && !hasMissingDimensions;
        }
        
        private static bool VerifyActualDimensions(IEnumerable<NetCdfDimension> actualDimensions,
                                                   IEnumerable<NetCdfDimension> expectedDimensions,
                                                   NetCdfFile actualNetCdfFile,
                                                   NetCdfFile expectedNetCdfFile,
                                                   string fileName,
                                                   ref string overallErrorMessage)
        {
            bool dimensionsAreSame = true;

            Dictionary<string, NetCdfDimension> expectedDimensionsLookup =
                expectedDimensions.ToDictionary(dimension => expectedNetCdfFile.GetDimensionName(dimension));

            foreach (NetCdfDimension actualDimension in actualDimensions)
            {
                string actualDimensionName = actualNetCdfFile.GetDimensionName(actualDimension);
                if (expectedDimensionsLookup.TryGetValue(actualDimensionName, out NetCdfDimension expectedDimension))
                {
                    bool isSameDimension = CompareDimension(actualDimension,
                                                            expectedDimension,
                                                            actualNetCdfFile,
                                                            expectedNetCdfFile,
                                                            fileName,
                                                            ref overallErrorMessage);
                    if (!isSameDimension)
                    {
                        dimensionsAreSame = false;
                    }
                }
                else
                {
                    dimensionsAreSame = false;
                    overallErrorMessage += $"The output file {fileName} contains an unexpected dimension: '{actualDimensionName}'.{Environment.NewLine}";
                }
            }

            return dimensionsAreSame;
        }
        
        private static bool CompareDimension(NetCdfDimension actualDimension,
                                             NetCdfDimension expectedDimension,
                                             NetCdfFile actualNetCdfFile,
                                             NetCdfFile expectedNetCdfFile,
                                             string fileName,
                                             ref string overallErrorMessage)
        {
            int actualDimensionLength = actualNetCdfFile.GetDimensionLength(actualDimension);
            int expectedDimensionLength = expectedNetCdfFile.GetDimensionLength(expectedDimension);

            if (actualDimensionLength == expectedDimensionLength)
            {
                return true;
            }

            string dimensionName = actualNetCdfFile.GetDimensionName(actualDimension);
            overallErrorMessage += $"The output file {fileName} contains a dimension '{dimensionName}' with length {actualDimensionLength}, " +
                                   $"but expected a length of {expectedDimensionLength}.{ Environment.NewLine}";

            return false;
        }
        
        private static bool CheckForMissingDimensions(IEnumerable<NetCdfDimension> actualDimensions,
                                                      IEnumerable<NetCdfDimension> expectedDimensions,
                                                      NetCdfFile actualNetCdfFile,
                                                      NetCdfFile expectedNetCdfFile,
                                                      string fileName,
                                                      ref string overallErrorMessage)
        {
            var hasMissingDimensions = false;

            Dictionary<string, NetCdfDimension> actualDimensionsLookup =
                actualDimensions.ToDictionary(dimension => actualNetCdfFile.GetDimensionName(dimension));
            foreach (NetCdfDimension expectedDimension in expectedDimensions)
            {
                string expectedDimensionName = expectedNetCdfFile.GetDimensionName(expectedDimension);

                if (!actualDimensionsLookup.TryGetValue(expectedDimensionName, out NetCdfDimension actualDimension))
                {
                    hasMissingDimensions = true;
                    overallErrorMessage += $"The output file {fileName} is missing the dimension '{expectedDimensionName}'.{Environment.NewLine}";
                }
            }

            return hasMissingDimensions;
        }
        
        private static bool CompareVariables(NetCdfFile actualNetCdfFile, 
                                             NetCdfFile expectedNetCdfFile, 
                                             string fileName, 
                                             ref string overallErrorMessage)
        {
            IEnumerable<NetCdfVariable> actualVariables = actualNetCdfFile.GetVariables();
            IEnumerable<NetCdfVariable> expectedVariables = expectedNetCdfFile.GetVariables();

            bool variablesAreSame = VerifyActualVariables(actualVariables,
                                                          expectedVariables,
                                                          actualNetCdfFile,
                                                          expectedNetCdfFile,
                                                          fileName,
                                                          ref overallErrorMessage);
            bool hasMissingVariables = CheckForMissingVariables(actualVariables,
                                                                expectedVariables,
                                                                actualNetCdfFile,
                                                                expectedNetCdfFile,
                                                                fileName,
                                                                ref overallErrorMessage);

            return variablesAreSame && !hasMissingVariables;
        }
        
        private static bool CheckForMissingVariables(IEnumerable<NetCdfVariable> actualVariables, 
                                                     IEnumerable<NetCdfVariable> expectedVariables, 
                                                     NetCdfFile actualNetCdfFile, 
                                                     NetCdfFile expectedNetCdfFile, 
                                                     string fileName, 
                                                     ref string overallErrorMessage)
        {
            var hasMissingVariables = false;

            Dictionary<string, NetCdfVariable> actualVariablesLookup =
                actualVariables.ToDictionary(variable => actualNetCdfFile.GetVariableName(variable));
            foreach (NetCdfVariable expectedVariable in expectedVariables)
            {
                string expectedVariableName = expectedNetCdfFile.GetVariableName(expectedVariable);

                if (!actualVariablesLookup.ContainsKey(expectedVariableName))
                {
                    hasMissingVariables = true;
                    overallErrorMessage += $"The output file {fileName} is missing the variable '{expectedVariableName}'.{Environment.NewLine}";
                }
            }

            return hasMissingVariables;
        }

        private static bool VerifyActualVariables(IEnumerable<NetCdfVariable> actualVariables, 
                                                  IEnumerable<NetCdfVariable> expectedVariables, 
                                                  NetCdfFile actualNetCdfFile, 
                                                  NetCdfFile expectedNetCdfFile, 
                                                  string fileName, 
                                                  ref string overallErrorMessage)
        {
            bool variablesAreSame = true;

            Dictionary<string, NetCdfVariable> expectedVariablesLookup =
                expectedVariables.ToDictionary(variable => expectedNetCdfFile.GetVariableName(variable));

            foreach (NetCdfVariable actualVariable in actualVariables)
            {
                string actualVariableName = actualNetCdfFile.GetVariableName(actualVariable);
                if (expectedVariablesLookup.TryGetValue(actualVariableName, out NetCdfVariable expectedVariable))
                {
                    bool isSameVariable = CompareVariable(actualVariable,
                                                          expectedVariable,
                                                          actualNetCdfFile,
                                                          expectedNetCdfFile,
                                                          fileName,
                                                          actualVariableName,
                                                          ref overallErrorMessage);
                    if (!isSameVariable)
                    {
                        variablesAreSame = false;
                    }
                }
                else
                {
                    variablesAreSame = false;
                    overallErrorMessage += $"The output file {fileName} contains an unexpected variable: '{actualVariableName}'.{Environment.NewLine}";
                }
            }

            return variablesAreSame;
        }

        private static bool CompareVariable(NetCdfVariable actualVariable,
                                            NetCdfVariable expectedVariable,
                                            NetCdfFile actualNetCdfFile,
                                            NetCdfFile expectedNetCdfFile,
                                            string fileName, 
                                            string variableName,
                                            ref string overallErrorMessage)
        {
            bool sameVariableDimensions = CompareVariableDimensions(actualVariable,
                                                                    expectedVariable,
                                                                    actualNetCdfFile,
                                                                    expectedNetCdfFile,
                                                                    fileName,
                                                                    variableName,
                                                                    ref overallErrorMessage);
            bool sameVariableAttributes = CompareVariableAttributes(actualVariable,
                                                                    expectedVariable,
                                                                    expectedNetCdfFile,
                                                                    actualNetCdfFile,
                                                                    fileName,
                                                                    variableName,
                                                                    ref overallErrorMessage);

            return sameVariableDimensions && sameVariableAttributes;
        }

        private static bool CompareVariableDimensions(NetCdfVariable actualVariable, 
                                                      NetCdfVariable expectedVariable,
                                                      NetCdfFile actualNetCdfFile,
                                                      NetCdfFile expectedNetCdfFile,
                                                      string fileName, 
                                                      string variableName, 
                                                      ref string overallErrorMessage)
        {
            int[] actualVariableShape = actualNetCdfFile.GetShape(actualVariable);
            int[] expectedVariableShape = expectedNetCdfFile.GetShape(expectedVariable);

            if (!actualVariableShape.SequenceEqual(expectedVariableShape))
            {
                overallErrorMessage += $"The output file {fileName} contains a variable '{variableName}' with shape " +
                                       $"'({string.Join(", ", actualVariableShape)})', but expected a shape " +
                                       $"of '({string.Join(", ", expectedVariableShape)})'.{Environment.NewLine}";
                return false;
            }

            return true;
        }

        private static bool CompareVariableAttributes(NetCdfVariable actualVariable, 
                                                      NetCdfVariable expectedVariable, 
                                                      NetCdfFile expectedNetCdfFile, 
                                                      NetCdfFile actualNetCdfFile, 
                                                      string fileName, 
                                                      string variableName,
                                                      ref string overallErrorMessage)
        {
            // We currently only support doubles
            NetCdfDataType actualVariableType = actualNetCdfFile.GetVariableDataType(actualVariable);
            NetCdfDataType expectedVariableType = expectedNetCdfFile.GetVariableDataType(expectedVariable);
            if (actualVariableType != NetCdfDataType.NcDoublePrecision || expectedVariableType != NetCdfDataType.NcDoublePrecision)
            {
                return true;
            }

            Dictionary<string, object> actualVariableAttributes = actualNetCdfFile.GetAttributes(actualVariable);
            Dictionary<string, object> expectedVariableAttributes = expectedNetCdfFile.GetAttributes(expectedVariable);

            bool attributesAreSame = VerifyActualAttributes(actualVariableAttributes,
                                                            expectedVariableAttributes,
                                                            fileName,
                                                            variableName,
                                                            ref overallErrorMessage);
            bool hasMissingAttributes = CheckForMissingAttributes(actualVariableAttributes,
                                                                 expectedVariableAttributes,
                                                                 fileName,
                                                                 variableName,
                                                                 ref overallErrorMessage);

            return attributesAreSame && !hasMissingAttributes;
        }

        private static bool VerifyActualAttributes(Dictionary<string, object> actualVariableAttributes,
                                                   Dictionary<string, object> expectedVariableAttributes,
                                                   string fileName, 
                                                   string variableName, 
                                                   ref string overallErrorMessage)
        {
            var isSameVariableAttribute = true;

            foreach (string actualVariableAttributeName in actualVariableAttributes.Keys)
            {
                if (expectedVariableAttributes.TryGetValue(actualVariableAttributeName, out object expectedVariableAttribute))
                {
                    isSameVariableAttribute = CompareAttribute(actualVariableAttributes[actualVariableAttributeName],
                                                               expectedVariableAttribute,
                                                               fileName,
                                                               variableName,
                                                               actualVariableAttributeName,
                                                               ref overallErrorMessage);
                }
                else
                {
                    overallErrorMessage += $"The output file {fileName} has a variable '{variableName}' that contains the unexpected attribute '{actualVariableAttributeName}'.{Environment.NewLine}";
                    isSameVariableAttribute = false;
                }
            }

            return isSameVariableAttribute;
        }

        private static bool CompareAttribute(object actualVariableAttribute,
                                             object expectedVariableAttribute,
                                             string fileName,
                                             string variableName,
                                             string attributeName,
                                             ref string overallErrorMessage)
        {
            bool isSameAttribute = true;

            if (!actualVariableAttribute.Equals(expectedVariableAttribute))
            {
                overallErrorMessage += $"The output file {fileName} has a variable '{variableName}' with an attribute '{attributeName}'" +
                                       $" that has a value of '{actualVariableAttribute}', but expected '{expectedVariableAttribute}'.{Environment.NewLine}";
                isSameAttribute = false;
            }

            return isSameAttribute;
        }

        private static bool CheckForMissingAttributes(Dictionary<string, object> actualVariableAttributes,
                                                      Dictionary<string, object> expectedVariableAttributes,
                                                      string fileName, 
                                                      string variableName, 
                                                      ref string overallErrorMessage)
        {
            var hasMissingAttributes = false;

            foreach (string expectedVariableAttributeName in expectedVariableAttributes.Keys)
            {
                if (!actualVariableAttributes.ContainsKey(expectedVariableAttributeName))
                {
                    overallErrorMessage += $"The output file {fileName} has a variable '{variableName}' that is missing the attribute '{expectedVariableAttributeName}'.{Environment.NewLine}";
                    hasMissingAttributes = true;
                }
            }

            return hasMissingAttributes;
        }
    }
}