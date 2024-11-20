using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders
{
    /// <summary>
    /// <see cref="IBcSectionParser"/> provides the help methods for parsing strings from Bc files.
    /// </summary>
    public interface IBcSectionParser
    {
        /// <summary>
        /// Complete given <paramref name="function"/> with given data.
        /// </summary>
        /// <param name="function">Function to complete.</param>
        /// <param name="argumentValues">Argument values to complete <paramref name="function"/> with.</param>
        /// <param name="values">Values to complete <paramref name="function"/> with.</param>
        /// <param name="periodic">ExtrapolationType for <paramref name="function"/>. </param>
        /// <typeparam name="T">Type of <paramref name="argumentValues"/>.</typeparam>
        /// <exception cref="ArgumentNullException">When any argument is Null (<paramref name="periodic"/> excluded).</exception>
        void CompleteFunction<T>(IFunction function, IEnumerable<T> argumentValues, IEnumerable<double> values, string periodic);

        /// <summary>
        /// Parsing of <paramref name="stringValues"/> to valid double out variable <paramref name="doubles"/>.
        /// </summary>
        /// <param name="stringValues">Doubles as string.</param>
        /// <param name="lineNumber">Current line used for logging.</param>
        /// <param name="doubles">Output of parsed <paramref name="stringValues"/>.</param>.
        /// <returns>True when all <paramref name="stringValues"/> are parsed correctly, false if one <paramref name="stringValues"/> is parsed incorrectly.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="stringValues"/> is Null</exception>
        bool TryParseDoubles(IEnumerable<string> stringValues, int lineNumber, out IEnumerable<double> doubles);

        /// <summary>
        /// Parsing of <paramref name="values"/> to valid datetime out variable <paramref name="dateTimes"/>.
        /// </summary>
        /// <param name="values">Dates and times as string</param>
        /// <param name="unitValue">Description for how <paramref name="dateTimes"/> should be parsed.</param>
        /// <param name="lineNumber">Current line used for logging.</param>
        /// <param name="dateTimes">Output of parsed <paramref name="values"/>.</param>
        /// <returns>True when all <paramref name="values"/> are parsed correctly, false if one <paramref name="values"/> is parsed incorrectly.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="values"/> or <paramref name="unitValue"/> is Null</exception>
        bool TryParseDateTimes(IEnumerable<string> values, string unitValue, int lineNumber, out IEnumerable<DateTime> dateTimes);

        /// <summary>
        /// Create a double from a table.
        /// </summary>
        /// <param name="table">Table to create const from.</param>
        /// <param name="lineNumber">Current line used for logging.</param>
        /// <returns>Constant from table, when no valid table entry available return 0.</returns>
        /// <exception cref="ArgumentNullException">When any <paramref name="table"/> is Null</exception>
        double CreateConstant(IList<IBcQuantityData> table, int lineNumber);
    }
}