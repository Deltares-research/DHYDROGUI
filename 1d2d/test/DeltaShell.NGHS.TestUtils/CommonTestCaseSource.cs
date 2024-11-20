using System;
using System.Collections.Generic;

namespace DeltaShell.NGHS.TestUtils
{
    /// <summary>
    /// Provides commonly used test case sources.
    /// </summary>
    public class CommonTestCaseSource
    {
        /// <summary>
        /// Provides cases for null or white space string.
        /// </summary>
        /// <returns>
        /// A collection containing <c>null</c>, an empty string, a string with only white space, and a new line.
        /// </returns>
        /// <example>
        /// Usage:
        /// <code>
        ///     [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        ///     </code>
        /// </example>
        public static IEnumerable<string> NullOrWhiteSpace()
        {
            yield return null;
            yield return "";
            yield return "    ";
            yield return Environment.NewLine;
        }
    }
}