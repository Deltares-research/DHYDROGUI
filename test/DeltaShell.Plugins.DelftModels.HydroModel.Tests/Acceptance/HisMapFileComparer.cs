using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.FunctionStores;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    public static class HisMapFileComparer
    {
        /// <summary>
        /// Compares if an actual his/map file is equal to an expected his/map file.
        /// </summary>
        /// <param name="actualFile">The actual his/map file.</param>
        /// <param name="expectedFile">The expected his/map file.</param>
        /// <param name="overallErrorMessage">Reference to an overall error message.</param>
        /// <returns><c>True</c> if the actual file is equal to the expected file.</returns>
        public static bool Compare(string actualFile, string expectedFile, ref string overallErrorMessage)
        {
            bool identical = true;
            string fileName = Path.GetFileName(actualFile);

            try
            {
                MapHisFileMetaData actualMetaData = MapHisFileReader.ReadMetaData(actualFile);
                MapHisFileMetaData expectedMetaData = MapHisFileReader.ReadMetaData(expectedFile);

                identical = CompareMetaData(actualMetaData, expectedMetaData, fileName, ref overallErrorMessage);
            }
            catch (Exception e)
            {
                overallErrorMessage += $"An exception occurred while comparing the output file {fileName}: {e.Message}";
                return false;
            }

            return identical;
        }

        private static bool CompareMetaData(MapHisFileMetaData actualMetaData, 
                                            MapHisFileMetaData expectedMetaData,
                                            string fileName,
                                            ref string overallErrorMessage)
        {
            bool hasSameParameters = CompareParameters(actualMetaData.Parameters, expectedMetaData.Parameters, fileName, ref overallErrorMessage);
            bool hasSameLocations = CompareLocations(actualMetaData.Locations, expectedMetaData.Locations, fileName, ref overallErrorMessage);
            bool hasSameTimes = CompareTimes(actualMetaData.Times, expectedMetaData.Times, fileName, ref overallErrorMessage);

            return hasSameParameters && hasSameLocations && hasSameTimes;
        }

        private static bool CompareTimes(IEnumerable<DateTime> actualTimes, 
                                         IEnumerable<DateTime> expectedTimes,
                                         string fileName,
                                         ref string overallErrorMessage)
        {
            IEnumerable<DateTime> missingExpectedTimes = expectedTimes.Except(actualTimes);
            IEnumerable<DateTime> unexpectedActualTimes = actualTimes.Except(expectedTimes);
            
            if (!missingExpectedTimes.Any() && !unexpectedActualTimes.Any())
            {
                return true;
            }
            
            foreach (DateTime missingExpectedTime in missingExpectedTimes)
            {
                overallErrorMessage += $"The output file {fileName} is missing the time '{missingExpectedTime}'.{Environment.NewLine}";
            }
            
            foreach (DateTime unexpectedActualTime in unexpectedActualTimes)
            {
                overallErrorMessage += $"The output file {fileName} contains the unexpected time '{unexpectedActualTime}'.{Environment.NewLine}";
            }

            return false;
        }

        private static bool CompareLocations(IEnumerable<string> actualLocations, 
                                             IEnumerable<string> expectedLocations, 
                                             string fileName, 
                                             ref string overallErrorMessage)
        {
            IEnumerable<string> missingExpectedLocations = expectedLocations.Except(actualLocations);
            IEnumerable<string> unexpectedActualLocations = actualLocations.Except(expectedLocations);
            
            if (!missingExpectedLocations.Any() && !unexpectedActualLocations.Any())
            {
                return true;
            }
            
            foreach (string missingExpectedLocation in missingExpectedLocations)
            {
                overallErrorMessage += $"The output file {fileName} is missing the location '{missingExpectedLocations}'.{Environment.NewLine}";
            }
            
            foreach (string unexpectedActualLocation in unexpectedActualLocations)
            {
                overallErrorMessage += $"The output file {fileName} contains the unexpected location '{unexpectedActualLocations}'.{Environment.NewLine}";
            }

            return false;
        }

        private static bool CompareParameters(IEnumerable<string> actualParameters, 
                                              IEnumerable<string> expectedParameters, 
                                              string fileName, 
                                              ref string overallErrorMessage)
        {
            IEnumerable<string> missingExpectedParameters = expectedParameters.Except(actualParameters);
            IEnumerable<string> unexpectedActualParameters = actualParameters.Except(expectedParameters);

            if (!missingExpectedParameters.Any() && !unexpectedActualParameters.Any())
            {
                return true;
            }

            foreach (string missingExpectedParameter in missingExpectedParameters)
            {
                overallErrorMessage += $"The output file {fileName} is missing the parameter '{missingExpectedParameter}'.{Environment.NewLine}";
            }
            
            foreach (string unexpectedActualParameter in unexpectedActualParameters)
            {
                overallErrorMessage += $"The output file {fileName} contains the unexpected parameter '{unexpectedActualParameter}'.{Environment.NewLine}";
            }

            return false;
        }
    }
}