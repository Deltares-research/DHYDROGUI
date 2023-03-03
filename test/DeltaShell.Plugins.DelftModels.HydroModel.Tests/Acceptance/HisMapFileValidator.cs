using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.FunctionStores;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    public static class HisMapFileValidator
    {
        /// <summary>
        /// Validates if an actual his/map file is equal to an expected his/map file.
        /// </summary>
        /// <param name="actualFile">The actual his/map file.</param>
        /// <param name="expectedFile">The expected his/map file.</param>
        /// <returns>Returns any validation issues.</returns>
        public static ICollection<string> Validate(string actualFile, string expectedFile)
        {
            string fileName = Path.GetFileName(actualFile);
            
            try
            {
                MapHisFileMetaData actualMetaData = MapHisFileReader.ReadMetaData(actualFile);
                MapHisFileMetaData expectedMetaData = MapHisFileReader.ReadMetaData(expectedFile);
                
                return CompareMetaData(actualMetaData, expectedMetaData, fileName).ToArray();
            }
            catch (Exception e)
            {
                return new[]
                {
                    $"An exception occurred while comparing the output file {fileName}: {e.Message}"
                };
            }
        }
        
        private static IEnumerable<string> CompareMetaData(MapHisFileMetaData actualMetaData,
                                                           MapHisFileMetaData expectedMetaData,
                                                           string fileName)
        {
            return CompareLists(actualMetaData.Parameters, expectedMetaData.Parameters, fileName)
                   .Concat(CompareLists(actualMetaData.Locations, expectedMetaData.Locations, fileName))
                   .Concat(CompareLists(actualMetaData.Times, expectedMetaData.Times, fileName));
        }
        
        private static IEnumerable<string> CompareLists<T>(IList<T> actual, IList<T> expected, string fileName)
        {
            foreach (T missingItem in expected.Except(actual))
            {
                yield return $"The output file '{fileName}' is missing the item '{missingItem}'.";
            }
        }
    }
}