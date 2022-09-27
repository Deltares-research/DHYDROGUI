using System;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Properties;

namespace DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders
{
    /// <summary>
    /// File reader to read time series.
    /// </summary>
    public class TimeSeriesFileReader : ITimeSeriesFileReader
    {
        private readonly ISpecificTimeSeriesFileReader[] readers;
        
        /// <summary>
        /// File reader to read time series.
        /// </summary>
        /// <param name="readers">File readers which can be used to read time series.</param>
        /// <exception cref="ArgumentNullException">Throws when the argument is null.</exception>
        public TimeSeriesFileReader(params ISpecificTimeSeriesFileReader[] readers)
        {
            Ensure.NotNull(readers, nameof(readers));
            if (readers.Length == 0)
            {
                throw new ArgumentException(string.Format(Resources.TimeSeriesFileReader_TimeSeriesFileReader_No_readers_in__0_, nameof(TimeSeriesFileReader)));
            }
            
            foreach (ISpecificTimeSeriesFileReader reader in readers)
            {
                Ensure.NotNull(reader, nameof(reader));
            }
            
            this.readers = readers;
        }
        
        public void Read(string propertyName, string filePath, IStructureTimeSeries structureTimeSeries, DateTime refDate)
        {
            Ensure.NotNull(propertyName,nameof(propertyName));
            Ensure.NotNull(filePath,nameof(filePath));
            Ensure.NotNull(structureTimeSeries,nameof(structureTimeSeries));

            try
            {
                readers.First(x => x.CanReadProperty(propertyName))
                       .Read(filePath, structureTimeSeries, refDate);
            }
            catch (Exception exception) when(exception is ArgumentException || exception is IOException || exception is FormatException)
            {
                throw new FileReadingException(message: exception.Message, inner:exception);
            }
        }

        public bool IsTimeSeriesProperty(string propertyName)
        {
            return readers.Any(x => x.CanReadProperty(propertyName));
        }
    }
}