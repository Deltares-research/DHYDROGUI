using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DeltaShell.NGHS.IO.TestUtils
{
    public static class FileComparer
    {
        public static bool Compare(string filePathExpected, string filePathActual, out string errorMessage, bool skipFirstLine = false)
        {
            errorMessage = string.Empty;
            if (!(File.Exists(filePathExpected) && File.Exists(filePathActual)))
            {
                errorMessage = string.Format("One or more files do not exist: '{0}' '{1}'", filePathExpected, filePathActual);
                return false;
            }

            var expectedText = ReadFile(filePathExpected).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var actualText = ReadFile(filePathActual).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (expectedText.Length != actualText.Length)
            {
                errorMessage = string.Format("Files are not the same length, expected: {0}, actual: {1}", expectedText.Length, actualText.Length);
                return false;
            }

            for (var i = skipFirstLine ? 1 : 0; i < expectedText.Length; i++)
            {
                var expectedTextLine = expectedText[i].RemoveTabs();
                var actualTextLine = actualText[i].RemoveTabs();
                if (string.CompareOrdinal(expectedTextLine, actualTextLine) != 0)
                {
                    errorMessage = string.Format("Expected: {0}{1}Found: {2} (approx line number: {3})", expectedTextLine, Environment.NewLine, actualTextLine, i);
                    return false;
                }
            }
            return true;
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

        private static string RemoveTabs(this string originalText)
        {
            return Regex.Replace(originalText, "[\t]", string.Empty);
        }
    }
}
