using System;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Boundary;

namespace DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders
{
    /// <summary>
    /// <see cref="TimSpecificTimeSeriesReader"/> Reader to read
    /// Timfile to <see cref="TimeSeries"/>.
    /// </summary>
    public class TimSpecificTimeSeriesReader : ISpecificTimeSeriesFileReader
    {
        private readonly ITimFileReader reader;
        
        public bool CanReadProperty(string propertyValue)
        {
            return propertyValue?.EndsWith(FileSuffices.TimFile) ?? false;
        }

        /// <summary>
        /// <see cref="TimSpecificTimeSeriesReader"/> Reader to read
        /// Timfile to <see cref="TimeSeries"/>.
        /// </summary>
        /// <param name="reader">Reader used to read the Timfile.</param>
        /// <exception cref="ArgumentNullException"> Throws when the argument is Null.</exception>
        public TimSpecificTimeSeriesReader(ITimFileReader reader)
        {
            Ensure.NotNull(reader, nameof(reader));
            this.reader = reader;
        }
        
        /// <summary>
        /// Read the time series from <paramref name="filePath"/> to <paramref name="structureTimeSeries"/>.
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <param name="structureTimeSeries"> The structure time series data .</param>
        /// <param name="refDate">The reference date used in determining the time series</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="structureTimeSeries"/> does not contain a valid time series function.
        /// </exception>
        public void Read(string filePath, IStructureTimeSeries structureTimeSeries, DateTime refDate)
        {
            reader.Read(filePath, structureTimeSeries.TimeSeries, refDate);
        }
    }
}