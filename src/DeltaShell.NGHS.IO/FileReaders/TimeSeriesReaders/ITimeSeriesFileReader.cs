using System;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.FileWriters.Boundary;

namespace DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders
{
    /// <summary>
    /// <see cref="ITimeSeriesFileReader"/> provides the read methods to read <see cref="TimeSeries"/>
    /// </summary>
    public interface ITimeSeriesFileReader
    {
        /// <summary>
        /// Method to read <see cref="TimeSeries"/> into <see cref="IFunction"/>.
        /// </summary>
        /// <param name="propertyName">Name of the file.</param>
        /// <param name="filePath">Path to the file which is to be read.</param>
        /// <param name="structureTimeSeries"> The structure time series data .</param>
        /// <param name="refDate">Reference date.</param>
        /// <exception cref="ArgumentNullException">Throws when any parameter is Null (<paramref name="refDate"/> excluded).</exception>
        /// <exception cref="FileReadingException">Throws when no readers are added or reader throw exception (see readers).</exception>
        void Read(string propertyName, string filePath, IStructureTimeSeries structureTimeSeries,
                  DateTime refDate);
        
        /// <summary>
        /// Method which determines whether the property is a time series or not.
        /// </summary>
        /// <param name="propertyName">Name of the file.</param>
        /// <returns>True when it is a time series, else false.</returns>
        bool IsTimeSeriesProperty(string propertyName);
    }
}