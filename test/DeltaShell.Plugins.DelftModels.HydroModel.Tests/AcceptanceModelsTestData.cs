using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Utils.IO;
using DHYDRO.Common.Extensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    /// <summary>
    /// Provides the acceptance models test data.
    /// </summary>
    public static class AcceptanceModelsTestData
    {
        /// <summary>
        /// Gets a collection of .mdu file based acceptance model test cases.
        /// </summary>
        /// <returns>A collection of test cases or an empty collection in case the test data could not be found.</returns>
        /// <remarks>The acceptance model test data is expected to be located in '&lt;repo_root&gt;\AcceptanceModels\Delft3DFM'.</remarks>
        public static IEnumerable<TestCaseData> GetAcceptanceModelTestCases()
        {
            return GetTestCases(GetAcceptanceModelsDirectory(), "*.mdu");
        }

        /// <summary>
        /// Gets a collection of DIMR .xml file based acceptance model test cases.
        /// </summary>
        /// <returns>A collection of test cases or an empty collection in case the test data could not be found.</returns>
        /// <remarks>The acceptance model test data is expected to be located in '&lt;repo_root&gt;\AcceptanceModelsRTCFMDimrXmlBased'.</remarks>
        public static IEnumerable<TestCaseData> GetDimrAcceptanceModelTestCases()
        {
            return GetTestCases(GetDimrAcceptanceModelsDirectory(), "dimr.xml");
        }

        private static DirectoryInfo GetAcceptanceModelsDirectory()
        {
            return new DirectoryInfo(Path.Combine(GetRepositoryDirectory(), "AcceptanceModels", "Delft3DFM"));
        }

        private static DirectoryInfo GetDimrAcceptanceModelsDirectory()
        {
            return new DirectoryInfo(Path.Combine(GetRepositoryDirectory(), "AcceptanceModelsRTCFMDimrXmlBased"));
        }

        private static string GetRepositoryDirectory()
        {
            return GetAssemblyDirectory().Parent?.Parent?.FullName;
        }

        private static DirectoryInfo GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return new DirectoryInfo(Path.GetDirectoryName(path));
        }

        private static IEnumerable<TestCaseData> GetTestCases(DirectoryInfo modelDirectory, string modelFileFilter)
        {
            string[] modelFiles = modelDirectory.EnumerateFiles(modelFileFilter, SearchOption.AllDirectories)
                                                .Select(x => x.FullName)
                                                .Where(x => !IsIgnored(x))
                                                .ToArray();

            foreach (string path in modelFiles)
            {
                string testName = GetTestName(modelDirectory, path);

                yield return new TestCaseData(path).SetName(testName);
            }
        }

        private static bool IsIgnored(string path)
        {
            return path.ContainsCaseInsensitive("_expected") ||
                   path.ContainsCaseInsensitive("_output") ||
                   path.ContainsCaseInsensitive("_00");
        }

        private static string GetTestName(DirectoryInfo testModelDirectory, string modelFilePath)
        {
            return FileUtils.GetRelativePath(testModelDirectory.FullName, modelFilePath);
        }
    }
}