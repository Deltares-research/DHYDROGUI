using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.FileWriters.Boundary;

namespace DeltaShell.NGHS.IO.FileWriters.TimeSeriesWriters
{
    /// <summary>
    /// <see cref="ITimeSeriesFileWriter"/> specifies the write methods to write
    /// <see cref="TimeSeries"/> to file
    /// </summary>
    public interface ITimeSeriesFileWriter
    {
        /// <summary>
        /// Write the time series from <paramref name="structureData"/> to <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <param name="structureData">Time series data associated with a structure</param>
        /// <param name="modelReferenceDate">The reference date used in determining the time series</param>
        /// <param name="commentLines">Optional comment lines used in commenting the time series</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> or <paramref name="structureData"/> is <c>null</c>.</exception>
        void Write(string filePath, 
                   IEnumerable<IStructureTimeSeries> structureData, 
                   DateTime modelReferenceDate,
                   IEnumerable<string> commentLines = null);

        /// <summary>
        /// Write a single time series from <paramref name="structureData"/> to <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <param name="structureName">Name of the structure</param>
        /// <param name="structureData">Structure time series data</param>
        /// <param name="modelReferenceDate">The reference date used in determining the time series</param>
        /// <param name="commentLines">The commentLines used in commenting the time series</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> or <paramref name="structureName"/> or <paramref name="modelReferenceDate"/> is <c>null</c>.</exception>
        void Write(string filePath, 
                   string structureName, 
                   ITimeSeries structureData, 
                   DateTime modelReferenceDate, 
                   IEnumerable<string> commentLines = null);
    }
}