using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    /// <summary>
    /// Helper class for comparing the contents of two FlowFM file directories.
    /// </summary>
    public static class FlowFmFileComparer
    {
        private static readonly string NcDumpExecutablePath;

        private static readonly string[] MduLinesToIgnore =
        {
            "# Generated on",
            "# Deltares,Delft3D FM 2018 Suite Version",
            "*",
            "Version",
            "GuiVersion",
            "TStart",
            "TStop",
            "RestartDateTime"
        };

        private static readonly string[] NetCdfLinesToIgnore =
        {
            ":history = \"Created on",
            ":source = \"D-Flow Flexible Mesh Plugin"
        };

        private static readonly string VerticalLine = $"==================================================================================={Environment.NewLine}";

        static FlowFmFileComparer()
        {
            NcDumpExecutablePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestUtils", "NetCDF", "ncdump.exe");
        }

        /// <summary>
        /// Compares the contents of two FlowFM file directories.
        /// </summary>
        /// <param name="expectedFlowFmFiles">The file paths of the expected FlowFM files.</param>
        /// <param name="actualFlowFmFiles">The file paths of the actual FlowFM files.</param>
        /// <param name="tempDirectory">A temporary working directory to use during the comparison.</param>
        public static void Compare(string[] expectedFlowFmFiles, string[] actualFlowFmFiles, string tempDirectory)
        {
            var identical = true;
            var actualFlowFmFileNames = actualFlowFmFiles.Select(Path.GetFileName);
            var expectedFlowFmFileNames = expectedFlowFmFiles.Select(Path.GetFileName);
            var allFileNames = actualFlowFmFileNames.Union(expectedFlowFmFileNames).ToArray();
            var overallErrorMessage = $"{Environment.NewLine}{VerticalLine}";

            foreach (var fileName in allFileNames)
            {
                var linesToIgnore = new string[] { };
                
                var expectedFlowFmFile = expectedFlowFmFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));
                if (expectedFlowFmFile == null)
                {
                    identical = false;

                    overallErrorMessage += $"The actual FlowFM file collection contains a file with name '{fileName}'; this file is not part of the expected collection of FlowFm files.{Environment.NewLine}{VerticalLine}";

                    continue;
                }

                var actualFlowFmFile = actualFlowFmFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));
                if (actualFlowFmFile == null)
                {
                    identical = false;

                    overallErrorMessage += $"The expected FlowFM file collection contains a file with name '{fileName}'; this file is not part of the actual collection of FlowFm files.{Environment.NewLine}{VerticalLine}";

                    continue;
                }

                switch (Path.GetExtension(expectedFlowFmFile))
                {
                    case ".mdu":
                    {
                        linesToIgnore = MduLinesToIgnore;
                        break;
                    }

                    case ".nc":
                    {
                        linesToIgnore = NetCdfLinesToIgnore;

                        expectedFlowFmFile = Path.Combine(tempDirectory, "ncdump", "expected", fileName);
                        actualFlowFmFile = Path.Combine(tempDirectory, "ncdump", "actual", fileName);

                        DumpNetCdfToTextFile(expectedFlowFmFile, expectedFlowFmFile);
                        DumpNetCdfToTextFile(actualFlowFmFile, actualFlowFmFile);

                        break;
                    }
                }

                identical = CompareFiles(expectedFlowFmFile, actualFlowFmFile, linesToIgnore, out var errorMessage) && identical;

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    overallErrorMessage += $"{errorMessage}{VerticalLine}";
                }
            }

            if (!identical)
            {
                Assert.Fail(overallErrorMessage);
            }
        }

        private static bool CompareFiles(
            string filePathExpected,
            string filePathActual,
            string[] linesToIgnore,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            ParseFile(filePathExpected, linesToIgnore, out var relevantLinesInExpectedText, out var ignoredLinesInExpectedText);
            ParseFile(filePathActual, linesToIgnore, out var relevantLinesInActualText, out var ignoredLinesInActualText);

            for (var i = 0; i < Math.Max(relevantLinesInExpectedText.Count, relevantLinesInActualText.Count); i++)
            {
                var expectedLine = relevantLinesInExpectedText.ElementAtOrDefault(i) ?? new Tuple<int, string>(-1, "<end of file>");
                var actualLine = relevantLinesInActualText.ElementAtOrDefault(i) ?? new Tuple<int, string>(-1, "<end of file>");

                if (string.CompareOrdinal(expectedLine.Item2, actualLine.Item2) != 0)
                {
                    errorMessage = $"Mismatch for FlowFM file '{Path.GetFileName(filePathExpected)}':" +
                                   $"{Environment.NewLine}" +
                                   $"{CreateErrorMessage(expectedLine, actualLine, ignoredLinesInExpectedText, ignoredLinesInActualText)}";

                    return false;
                }
            }

            return true;
        }

        private static void ParseFile(
            string filePath,
            string[] linesToIgnore,
            out List<Tuple<int, string>> relevantLines,
            out List<Tuple<int, string>> ignoredLines)
        {
            relevantLines = new List<Tuple<int, string>>();
            ignoredLines = new List<Tuple<int, string>>();

            var text = ReadFile(filePath).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < text.Length; i++)
            {
                var lineNumber = i + 1;
                var lineWithoutTabs = RemoveTabs(text[i]);

                if (linesToIgnore.Any(ignore => lineWithoutTabs.StartsWith(ignore)))
                {
                    ignoredLines.Add(new Tuple<int, string>(lineNumber, lineWithoutTabs));
                }
                else
                {
                    relevantLines.Add(new Tuple<int, string>(lineNumber, lineWithoutTabs));
                }
            }
        }

        private static string CreateErrorMessage(
            Tuple<int, string> expectedLine,
            Tuple<int, string> actualLine,
            IReadOnlyCollection<Tuple<int, string>> ignoredLinesInExpectedText,
            IEnumerable<Tuple<int, string>> ignoredLinesInActualText)
        {
            var errorMessage = $"Expected: {expectedLine.Item2} [Line {expectedLine.Item1}]{Environment.NewLine}Actual:   {actualLine.Item2} [Line {actualLine.Item1}]{Environment.NewLine}";

            if (ignoredLinesInExpectedText.Any())
            {
                errorMessage += $"{Environment.NewLine}Note that the following lines are ignored in the expected file:{Environment.NewLine}";
                errorMessage = ignoredLinesInExpectedText.Aggregate(errorMessage, (current, ignoredLine) => current + $"[Line {ignoredLine.Item1}] {ignoredLine.Item2}{Environment.NewLine}");
            }

            if (ignoredLinesInExpectedText.Any())
            {
                errorMessage += $"{Environment.NewLine}Note that the following lines are ignored in the actual file:{Environment.NewLine}";
                errorMessage = ignoredLinesInActualText.Aggregate(errorMessage, (current, ignoredLine) => current + $"[Line {ignoredLine.Item1}] {ignoredLine.Item2}{Environment.NewLine}");
            }

            return errorMessage;
        }

        private static string ReadFile(string filePath)
        {
            string buffer;

            using (var fileStream = new StreamReader(filePath))
            {
                buffer = fileStream.ReadToEnd();
            }

            return buffer;
        }

        private static string RemoveTabs(string originalText)
        {
            return Regex.Replace(originalText, "[\t]", string.Empty);
        }

        private static void DumpNetCdfToTextFile(string netCdfFilePath, string targetFilePath)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = NcDumpExecutablePath;
                process.StartInfo.Arguments = netCdfFilePath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
                File.WriteAllText(targetFilePath, process.StandardOutput.ReadToEnd());

                process.WaitForExit();
            }
        }
    }
}