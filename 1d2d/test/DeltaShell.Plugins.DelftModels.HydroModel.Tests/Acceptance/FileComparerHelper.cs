using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    public static class FileComparerHelper
    {
        private static readonly string NcDumpExecutablePath;
        
        static FileComparerHelper()
        {
            NcDumpExecutablePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestUtils", "NetCDF", "ncdump.exe");
        }
        
        /// <summary>
        /// Compares if two files are equal.
        /// </summary>
        /// <param name="filePathExpected">Filepath to the reference file.</param>
        /// <param name="filePathActual">Filepath to the actual file.</param>
        /// <param name="linesToIgnore">Lines to ignore.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="filePathExpected"/>, <paramref name="filePathActual"/> or
        /// <paramref name="linesToIgnore"/> is <c>null</c>.</exception>
        public static void CompareFiles(string filePathExpected, string filePathActual, string[] linesToIgnore)
        {
            ParseFile(filePathExpected, linesToIgnore, out var relevantLinesInExpectedText, out var ignoredLinesInExpectedText);
            ParseFile(filePathActual, linesToIgnore, out var relevantLinesInActualText, out var ignoredLinesInActualText);
            
            GetMismatchingLines(relevantLinesInExpectedText, relevantLinesInActualText, out var mismatchingLinesInExpected, out var mismatchingLinesInActual);
            
            RemoveEquivalentLines(mismatchingLinesInExpected, mismatchingLinesInActual);

            if (mismatchingLinesInExpected.Any())
            {
                string message = $"Mismatch for file '{Path.GetFileName(filePathExpected)}':" +
                                 $"{Environment.NewLine}" +
                                 $"{CreateErrorMessage(mismatchingLinesInExpected.First(), mismatchingLinesInActual.First(), ignoredLinesInExpectedText, ignoredLinesInActualText)}";

                Assert.Fail(message);
            }
        }
        
        /// <summary>
        /// Dumps the provided netcdf file to a text file.
        /// </summary>
        /// <param name="netCdfFilePath">Filepath to the netcdf file.</param>
        /// <param name="targetFilePath">Filepath to the target file.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        public static void DumpNetCdfToTextFile(string netCdfFilePath, string targetFilePath)
        {
            if (netCdfFilePath == null)
            {
                throw new ArgumentNullException(nameof(netCdfFilePath));
            }

            if (targetFilePath == null)
            {
                throw new ArgumentNullException(nameof(targetFilePath));
            }
            
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

        /// <summary>
        /// Parses the provided file.
        /// </summary>
        /// <param name="filePath">The file to parse.</param>
        /// <param name="linesToIgnore">Any lines to ignore,</param>
        /// <param name="relevantLines">The relevant parsed lines as output variable.</param>
        /// <param name="ignoredLines">The ignored parsed lines as output variable.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="filePath"/> or <paramref name="linesToIgnore"/> is <c>null</c>.</exception>
        public static void ParseFile(
            string filePath,
            string[] linesToIgnore,
            out List<Tuple<int, string>> relevantLines,
            out List<Tuple<int, string>> ignoredLines)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (linesToIgnore == null)
            {
                throw new ArgumentNullException(nameof(linesToIgnore));
            }

            relevantLines = new List<Tuple<int, string>>();
            ignoredLines = new List<Tuple<int, string>>();

            var text = ReadFile(filePath).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < text.Length; i++)
            {
                var lineNumber = i + 1;
                var lineWithoutTabs = RemoveLeadingWhitespace(text[i]);

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

        /// <summary>
        /// Creates an error message based on an expected and actual parsed line.
        /// </summary>
        /// <param name="expectedLine">The expected parsed line.</param>
        /// <param name="actualLine">The actual parsed line.</param>
        /// <returns>The error message.</returns>
        /// <exception cref="ArgumentNullException">When any parameter is <c>null</c> or when the string value of either tuple is <c>null</c>.</exception>
        public static string CreateErrorMessage(
            Tuple<int, string> expectedLine,
            Tuple<int, string> actualLine)
        {
            if (expectedLine?.Item2 == null)
            {
                throw new ArgumentNullException(nameof(expectedLine));
            }

            if (actualLine?.Item2 == null)
            {
                throw new ArgumentNullException(nameof(actualLine));
            }

            return CreateErrorMessage(expectedLine, actualLine, new List<Tuple<int, string>>(), new List<Tuple<int, string>>());
        }

        /// <summary>
        /// Gets mismatching lines between a collection of expected lines and a collection of actual lines.
        /// </summary>
        /// <param name="relevantLinesInExpectedText">The collection of expected lines.</param>
        /// <param name="relevantLinesInActualText">The collection of actual lines.</param>
        /// <param name="mismatchingLinesInExpected">A collection of mismatching lines in the collection of expected lines as output variable.</param>
        /// <param name="mismatchingLinesInActual">A collection of mismatching lines in the collection of actual lines as output variable.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="relevantLinesInExpectedText"/> or <paramref name="relevantLinesInActualText"/> is <c>null</c>.</exception>
        public static void GetMismatchingLines(
            IReadOnlyCollection<Tuple<int, string>> relevantLinesInExpectedText,
            IReadOnlyCollection<Tuple<int, string>> relevantLinesInActualText,
            out List<Tuple<int, string>> mismatchingLinesInExpected,
            out List<Tuple<int, string>> mismatchingLinesInActual)
        {
            if (relevantLinesInExpectedText == null)
            {
                throw new ArgumentNullException(nameof(relevantLinesInExpectedText));
            }

            if (relevantLinesInActualText == null)
            {
                throw new ArgumentNullException(nameof(relevantLinesInActualText));
            }

            mismatchingLinesInExpected = new List<Tuple<int, string>>();
            mismatchingLinesInActual = new List<Tuple<int, string>>();

            for (var i = 0; i < Math.Max(relevantLinesInExpectedText.Count, relevantLinesInActualText.Count); i++)
            {
                var expectedLine = relevantLinesInExpectedText.ElementAtOrDefault(i) ?? CreateDummyLine();
                var actualLine = relevantLinesInActualText.ElementAtOrDefault(i) ?? CreateDummyLine();

                if (string.CompareOrdinal(expectedLine.Item2, actualLine.Item2) != 0)
                {
                    mismatchingLinesInExpected.Add(expectedLine);
                    mismatchingLinesInActual.Add(actualLine);
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

        /// <summary>
        /// Removes equivalent lines from a collection of expected lines and a collection of actual lines.
        /// </summary>
        /// <param name="mismatchingLinesInExpected">The collection of mismatching lines in the collection of expected lines.</param>
        /// <param name="mismatchingLinesInActual">The collection of mismatching lines in the collection of actual lines.</param>
        /// <exception cref="ArgumentNullException">When any parameter is <c>null</c>.</exception>
        /// <exception cref="NotSupportedException">When the intersect of both collections is not the same.</exception>
        public static void RemoveEquivalentLines(
            ICollection<Tuple<int, string>> mismatchingLinesInExpected,
            ICollection<Tuple<int, string>> mismatchingLinesInActual)
        {
            if (mismatchingLinesInExpected == null)
            {
                throw new ArgumentNullException(nameof(mismatchingLinesInExpected));
            }

            if (mismatchingLinesInActual == null)
            {
                throw new ArgumentNullException(nameof(mismatchingLinesInActual));
            }

            var lineEqualityComparer = new LineEqualityComparer();
            var equivalentLinesInExpected = mismatchingLinesInExpected.Intersect(mismatchingLinesInActual, lineEqualityComparer).ToList();
            var equivalentLinesInActual = mismatchingLinesInActual.Intersect(mismatchingLinesInExpected, lineEqualityComparer).ToList();

            if (equivalentLinesInExpected.Count != equivalentLinesInActual.Count)
            {
                throw new NotSupportedException("Extend comparison algorithm when getting here...");
            }

            foreach (var equivalentLineInExpected in equivalentLinesInExpected)
            {
                mismatchingLinesInExpected.Remove(equivalentLineInExpected);
            }

            foreach (var equivalentLineInActual in equivalentLinesInActual)
            {
                mismatchingLinesInActual.Remove(equivalentLineInActual);
            }
        }

        /// <summary>
        /// Sorts the given .bc files based on the provided key.
        /// </summary>
        /// <param name="expectedFile">The expected .bc file to sort.</param>
        /// <param name="actualFile">The actual .bc file to sort.</param>
        /// <param name="key">The name of the key to sort on.</param>
        public static void SortBcFilesByKey(string expectedFile, string actualFile, string key)
        {
            IEnumerable<IniSection> readSections = new BcReader(new FileSystem()).ReadBcFile(expectedFile).Select(c => c.Section);
            File.Delete(expectedFile);
            new IniWriter().WriteIniFile(readSections.OrderBy(s => s.ReadProperty<string>(key)), expectedFile);

            readSections = new BcReader(new FileSystem()).ReadBcFile(actualFile).Select(c => c.Section);
            File.Delete(actualFile);
            new IniWriter().WriteIniFile(readSections.OrderBy(s => s.ReadProperty<string>(key)), actualFile);
        }
        
        private class LineEqualityComparer : IEqualityComparer<Tuple<int, string>>
        {
            public bool Equals(Tuple<int, string> first, Tuple<int, string> second)
            {
                return first.Item2.Equals(second.Item2);
            }

            public int GetHashCode(Tuple<int, string> obj)
            {
                return obj.Item2.GetHashCode();
            }
        }
        
        private static Tuple<int, string> CreateDummyLine()
        {
            return new Tuple<int, string>(-1, "<end of file>");
        }

        private static string RemoveLeadingWhitespace(string originalText)
        {
            return Regex.Replace(originalText, "^[ \t]+", string.Empty);
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
    }
}