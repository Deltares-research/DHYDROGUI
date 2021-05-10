using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    public static class InputFileComparer
    {
        /// <summary>
        /// Compares the files of two input directories.
        /// </summary>
        /// <remarks>
        /// For specific files, you can ignore lines starting with a specific string by providing a lookup of files mapping to a collection of strings.
        /// </remarks>
        /// <param name="expectedSaveProjectDirectory">Path to directory containing actual input files to compare.</param>
        /// <param name="actualSaveProjectDirectory">Path to directory containing expected input files to compare.</param>
        /// <param name="mduFileName">Name of the mdu file that corresponds with the folder name where the FlowFM data is located.</param>
        /// <param name="tempDirectory">Path to temporary directory.</param>
        /// <param name="hasRrData">Whether or not Rainfall Runoff data should be compared.</param>
        /// <param name="flowFmLinesToIgnorePerFile">Lookup for which lines should be ignored per FlowFM file.</param>
        /// <param name="rainfallRunoffLinesToIgnorePerFile">Lookup for which lines should be ignored per Rainfall Runoff file.</param>
        public static void CompareInputDirectories(string expectedSaveProjectDirectory,
                                                     string actualSaveProjectDirectory,
                                                     string mduFileName,
                                                     string tempDirectory,
                                                     bool hasRrData,
                                                     IReadOnlyDictionary<string, IEnumerable<string>> flowFmLinesToIgnorePerFile,
                                                     IReadOnlyDictionary<string, IEnumerable<string>> rainfallRunoffLinesToIgnorePerFile)
        {
            Console.WriteLine("Comparing FlowFM input data");
            string flowFmInitialSaveDirectory = Path.Combine(expectedSaveProjectDirectory, mduFileName, "input");
            string flowFmSecondSaveDirectory = Path.Combine(actualSaveProjectDirectory, mduFileName, "input");
            CompareFlowFMInputFiles(flowFmInitialSaveDirectory, flowFmSecondSaveDirectory, tempDirectory, flowFmLinesToIgnorePerFile);

            if (hasRrData)
            {
                Console.WriteLine("Comparing Rainfall Runoff input data");
                string rrInitialSaveDirectory = Path.Combine(expectedSaveProjectDirectory, "Rainfall Runoff");
                string rrSecondSaveDirectory = Path.Combine(actualSaveProjectDirectory, "Rainfall Runoff");
                CompareRainfallRunoffInputFiles(rrInitialSaveDirectory, rrSecondSaveDirectory, rainfallRunoffLinesToIgnorePerFile);
            }
        }
        
        /// <summary>
        /// Compares the files of two input directories.
        /// </summary>
        /// <param name="expectedSaveProjectDirectory">Path to directory containing first input files to compare.</param>
        /// <param name="actualSaveProjectDirectory">Path to directory containing second input files to compare.</param>
        /// <param name="mduFileName">Name of the mdu file that corresponds with the folder name where the FlowFM data is located.</param>
        /// <param name="tempDirectory">Path to temporary directory.</param>
        /// <param name="hasRrData">Whether or not Rainfall Runoff data should be compared.</param>
        public static void CompareInputDirectories(string expectedSaveProjectDirectory,
                                                     string actualSaveProjectDirectory,
                                                     string mduFileName,
                                                     string tempDirectory,
                                                     bool hasRrData)
        {
            var linesToIgnore = new Dictionary<string, IEnumerable<string>>(); // don't ignore anything
            CompareInputDirectories(expectedSaveProjectDirectory, 
                                      actualSaveProjectDirectory, 
                                      mduFileName, 
                                      tempDirectory, 
                                      hasRrData, 
                                      linesToIgnore, 
                                      linesToIgnore);
        }
        
        private static void CompareFlowFMInputFiles(string flowFmInitialSaveDirectory, 
                                               string flowFmSecondSaveDirectory, 
                                               string tempDirectory, 
                                               IReadOnlyDictionary<string, IEnumerable<string>> flowFmLinesToIgnorePerFile)
        {
            string[] flowFmInitialSaveFiles = Directory.GetFiles(flowFmInitialSaveDirectory);
            if (!flowFmInitialSaveFiles.Any())
            {
                Assert.Fail($"No saved files (first save) could be found at {flowFmInitialSaveDirectory}.");
            }
            
            string[] flowFmSecondSaveFiles = Directory.GetFiles(flowFmSecondSaveDirectory);
            if (!flowFmSecondSaveFiles.Any())
            {
                Assert.Fail($"No saved files (second save) could be found at {flowFmSecondSaveDirectory}.");
            }
            
            FlowFmFileComparer.Compare(flowFmInitialSaveFiles, flowFmSecondSaveFiles, tempDirectory, flowFmLinesToIgnorePerFile);
        }

        private static void CompareRainfallRunoffInputFiles(string rrInitialSaveDirectory,
                                                       string rrSecondSaveDirectory,
                                                       IReadOnlyDictionary<string, IEnumerable<string>> rainfallRunoffLinesToIgnorePerFile)
        {
            string[] rrInitialSaveFiles = Directory.GetFiles(rrInitialSaveDirectory);
            if (!rrInitialSaveFiles.Any())
            {
                Assert.Fail($"No saved files (first save) could be found at {rrInitialSaveDirectory}.");
            }
            
            string[] rrSecondSaveFiles = Directory.GetFiles(rrSecondSaveDirectory);
            if (!rrSecondSaveFiles.Any())
            {
                Assert.Fail($"No saved files (second save) could be found at {rrSecondSaveDirectory}.");
            }

            RainfallRunoffFileComparer.Compare(AcceptanceModelTestHelper.FilterInputFiles(rrInitialSaveFiles).ToArray(),
                                               AcceptanceModelTestHelper.FilterInputFiles(rrSecondSaveFiles).ToArray(),
                                               rainfallRunoffLinesToIgnorePerFile);
        }
    }
}