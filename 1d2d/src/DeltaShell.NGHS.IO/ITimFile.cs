using System;
using DelftTools.Functions;

namespace DeltaShell.NGHS.IO
{
    /// <summary>
    /// <see cref="ITimFileReader"/> provides the read methods to read
    /// '.tim' file to <see cref="TimeSeries"/>
    /// </summary>
    public interface ITimFileReader
    {
        /// <summary>
        /// Read the time series from <paramref name="timFilePath"/> to <paramref name="function"/>.
        /// </summary>
        /// <param name="timFilePath">The path of the .tim file</param>
        /// <param name="function">The function to write the time series values to</param>
        /// <param name="refDate">The reference date used in determining the time series</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="function"/> is not a valid time series function.
        /// </exception>
        void Read(string timFilePath, IFunction function, DateTime? refDate);
        
        /// <summary>
        /// Read the <see cref="TimeSeries"/> from <paramref name="timFilePath"/>.
        /// </summary>
        /// <param name="timFilePath">The path of the .tim file</param>
        /// <param name="modelReferenceDate">The reference date used in determining the time series</param>
        /// <returns>
        /// The <see cref="TimeSeries"/> at <paramref name="timFilePath"/>.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown when the tim file is incorrectly formatted.
        /// </exception>
        TimeSeries Read(string timFilePath, DateTime modelReferenceDate);
    }
}