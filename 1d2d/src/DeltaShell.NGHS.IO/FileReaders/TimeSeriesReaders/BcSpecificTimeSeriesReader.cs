using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;

namespace DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders
{
    /// <summary>
    /// <see cref="BcSpecificTimeSeriesReader"/> Reader to read
    /// Bcfile to <see cref="TimeSeries"/>.
    /// </summary>
    public class BcSpecificTimeSeriesReader : ISpecificTimeSeriesFileReader
    {
        private readonly IBcReader reader;
        private readonly IBcSectionParser parser;
        private readonly ILogHandler logHandler;
        private Dictionary<string, BcIniSection[]> structureDictionary;
        
        public bool CanReadProperty(string propertyValue)
        {
            return propertyValue?.EndsWith(FileSuffices.BcFile) ?? false;
        }

        /// <summary>
        /// <see cref="BcSpecificTimeSeriesReader"/> Reader to read
        /// Bcfile to <see cref="TimeSeries"/>.
        /// </summary>
        /// <param name="reader">Reader used to read the Bcfile.</param>
        /// <param name="parser">Parser used to parse Bcfile data.</param>
        /// <param name="logHandler">Log handler used for logging warnings</param>
        /// <exception cref="ArgumentNullException"> Throws when any of the arguments are Null.</exception>
        public BcSpecificTimeSeriesReader(IBcReader reader, IBcSectionParser parser, ILogHandler logHandler)
        {
            Ensure.NotNull(reader, nameof(reader));
            Ensure.NotNull(parser, nameof(parser));
            Ensure.NotNull(logHandler, nameof(logHandler));

            this.reader = reader;
            this.parser = parser;
            this.logHandler = logHandler;
        }
        
        /// <summary>
        /// Read the time series from <paramref name="filePath"/> to <paramref name="structureTimeSeries"/>.
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <param name="structureTimeSeries"> The structure time series data.</param>
        /// <param name="refDate">The reference date used in determining the time series</param>
        /// <exception cref="IOException"><paramref name="iniFile"/> includes an incorrect or invalid syntax for file name, directory name, or volume label.</exception>
        /// <exception cref="FormatException">When an invalid line was encountered.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> or <paramref name="structureTimeSeries"/> is null</exception>
        public void Read(string filePath, IStructureTimeSeries structureTimeSeries, DateTime refDate)
        {
            Ensure.NotNull(filePath, nameof(filePath));
            Ensure.NotNull(structureTimeSeries, nameof(structureTimeSeries));

            CacheDataFromBcFile(filePath);

            string retrievedQuantity = QuantityHelper.GetQuantity(structureTimeSeries.Structure, structureTimeSeries.TimeSeries.Name);

            BcIniSection retrievedStructureIniSection = RetrieveStructuresSection(structureTimeSeries.Structure.Name, retrievedQuantity);

            if (retrievedStructureIniSection == null)
            {
                logHandler.ReportWarning(string.Format(Resources.BcSpecificTimeSeriesReader_Read_No_structure_found_with_name__0__quantity__1__in_file__2_, 
                                                       structureTimeSeries.Structure.Name, 
                                                       retrievedQuantity,  
                                                       filePath));
                return;
            }

            ReadTimeSeriesIntoFunction(structureTimeSeries.TimeSeries, retrievedStructureIniSection);
        }

        private void CacheDataFromBcFile(string filePath)
        {
            if (structureDictionary == null)
            {
                IEnumerable<BcIniSection> structuresFromFile = reader.ReadBcFile(filePath);
                structureDictionary = ConvertStructureListToDictionary(structuresFromFile);
            }
        }

        private static Dictionary<string, BcIniSection[]> ConvertStructureListToDictionary(IEnumerable<BcIniSection> structuresFromFile)
        {
            return GroupStructuresByNameAndNotNull(structuresFromFile).ToDictionary(group => @group.Key, grouping => grouping.ToArray());
        }

        private static IEnumerable<IGrouping<string, BcIniSection>> GroupStructuresByNameAndNotNull(IEnumerable<BcIniSection> structuresFromFile)
        {
            IEnumerable<IGrouping<string, BcIniSection>> structuresGroupedByName = structuresFromFile.GroupBy(section => section.Section.GetPropertyValueWithOptionalDefaultValue("name"));
            return structuresGroupedByName.Where(group => @group.Key != null);
        }

        private BcIniSection RetrieveStructuresSection(string structureName, string relevantQuantity)
        {
            if (structureDictionary.TryGetValue(structureName, out BcIniSection[] bcIniSections))
            {
                return bcIniSections.FirstOrDefault(section => section.Table.Any(data => data.Quantity.Value.Equals(relevantQuantity)));
            }

            return null;
        }

        private void ReadTimeSeriesIntoFunction(ITimeSeries givenTimeSeries, BcIniSection bcIniSection)
        {
            IList<IBcQuantityData> table = bcIniSection.Table;
            CreateTimeSeries(table, table.First().Unit.Value, givenTimeSeries, bcIniSection.Section.LineNumber);
        }

        private void CreateTimeSeries(IList<IBcQuantityData> table, string periodic, ITimeSeries givenTimeSeries, int lineNumber)
        {
            if (parser.TryParseDateTimes(table[0].Values, table[0].Unit.Value, lineNumber, out IEnumerable<DateTime> argumentValues) 
                && parser.TryParseDoubles(table[1].Values, lineNumber, out IEnumerable<double> functionValues))
            {
                parser.CompleteFunction(givenTimeSeries, argumentValues, functionValues, periodic);
            }
        }
    }
}
