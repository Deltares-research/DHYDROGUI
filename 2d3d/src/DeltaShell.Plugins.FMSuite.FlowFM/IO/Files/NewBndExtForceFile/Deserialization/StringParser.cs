using System.Globalization;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization
{
    /// <summary>
    /// Parser for strings from the user input.
    /// </summary>
    public static class StringParser
    {
        /// <summary>
        /// Try to parse the provided string to a double allowing any number style.
        /// </summary>
        /// <param name="stringValue"> A string containing a number to convert. </param>
        /// <param name="result">
        /// When this method returns, contains a double-precision floating-point number equivalent of the
        /// numeric value or symbol contained in <paramref name="stringValue"/>, if the conversion succeeded, or zero if the
        /// conversion failed.
        /// </param>
        /// <returns><c>true</c> if <paramref name="stringValue"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParseToDouble(this string stringValue, out double result)
        {
            return double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
    }
}