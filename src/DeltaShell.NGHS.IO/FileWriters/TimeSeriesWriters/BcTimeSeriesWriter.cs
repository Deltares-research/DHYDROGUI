using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.TimeSeriesWriters
{
    /// <summary>
    /// <see cref="BcTimeSeriesWriter"/> implements the <see cref="ITimeSeriesFileWriter"/> for .bc time
    /// series files.
    /// </summary>
    /// <seealso cref="ITimeSeriesFileWriter"/>
    public class BcTimeSeriesWriter : ITimeSeriesFileWriter
    {
        private readonly IBcFileWriter writer;
        private readonly IStructureBoundaryGenerator structureBoundaryGenerator;

        /// <summary>
        /// Creates a new <see cref="BcTimeSeriesWriter"/> with the given parameters.
        /// </summary>
        /// <param name="writer">The bc file writer.</param>
        /// <param name="structureBoundaryGenerator">The boundary data generator</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public BcTimeSeriesWriter(IBcFileWriter writer, IStructureBoundaryGenerator structureBoundaryGenerator)
        {
            Ensure.NotNull(writer, nameof(writer));
            Ensure.NotNull(structureBoundaryGenerator, nameof(structureBoundaryGenerator));
            this.writer = writer;
            this.structureBoundaryGenerator = structureBoundaryGenerator;
        }
        
        public void Write(string filePath, IEnumerable<IStructureTimeSeries> structureData, DateTime modelReferenceDate, IEnumerable<string> commentLines = null)
        {
            Ensure.NotNull(filePath, nameof(filePath));
            Ensure.NotNull(structureData, nameof(structureData));

            IEnumerable<DelftBcCategory> boundaries = structureBoundaryGenerator.GenerateBoundaries(structureData, modelReferenceDate);

            FileUtils.DeleteIfExists(filePath);
            WriteToBcFile(filePath, GetDataWithHeader(boundaries));
        }

        public void Write(string filePath, string structureName, ITimeSeries structureData, DateTime modelReferenceDate, IEnumerable<string> commentLines = null)
        {
            Ensure.NotNull(filePath, nameof(filePath));
            Ensure.NotNull(structureName, nameof(structureName));
            Ensure.NotNull(structureData, nameof(structureData));
            
            IEnumerable<DelftBcCategory> boundary = structureBoundaryGenerator.GenerateBoundary(structureName, structureData, modelReferenceDate);

            WriteToBcFile(filePath, boundary);
        }

        private static IEnumerable<DelftBcCategory> GetDataWithHeader(IEnumerable<DelftBcCategory> dataToWrite)
        {
            IniSection generalRegion = GeneralRegionGenerator.GenerateGeneralRegion(
                GeneralRegion.BoundaryConditionsMajorVersion, GeneralRegion.BoundaryConditionsMinorVersion,
                GeneralRegion.FileTypeName.BoundaryConditions);

            List<DelftBcCategory> listOfData = new List<DelftBcCategory>();
            listOfData.Add(new DelftBcCategory(generalRegion));
            listOfData.AddRange(dataToWrite);
            
            return listOfData;
        }

        private void WriteToBcFile(string filePath, IEnumerable<DelftBcCategory> dataToWrite)
        {
            if (dataToWrite.Any())
            {
                writer.WriteBcFile(dataToWrite, filePath);
            }
        }
    }
}