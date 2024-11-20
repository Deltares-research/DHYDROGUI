using System;
using System.IO;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.FileWriters.Boundary;

namespace DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders
{
    /// <summary>
    /// <see cref="ISpecificTimeSeriesFileReader"/> provides the read methods to read
    /// file to <see cref="TimeSeries"/>
    /// </summary>
    public interface ISpecificTimeSeriesFileReader
    {
        /// <summary>
        /// Read the time series from <paramref name="filePath"/> to <paramref name="structureTimeSeries"/>.
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <param name="structureTimeSeries"> The structure time series data .</param>
        /// <param name="refDate">The reference date used in determining the time series</param>
        /// <remarks>This read method is setup conform the TimFile read method.</remarks>
        /// <exception cref="FileReadingException">Thrown when reader throws one of the following exceptions (see specific readers for more information).</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="FormatException"></exception>
        void Read(string filePath, IStructureTimeSeries structureTimeSeries,
                  DateTime refDate);

        /// <summary>
        /// Determines if a property can be read.
        /// </summary>
        /// <param name="propertyValue">Name of the file the data is stored in.</param>
        /// <returns>True if the file is a valid readable file, else false</returns>
        bool CanReadProperty(string propertyValue);
    }
}