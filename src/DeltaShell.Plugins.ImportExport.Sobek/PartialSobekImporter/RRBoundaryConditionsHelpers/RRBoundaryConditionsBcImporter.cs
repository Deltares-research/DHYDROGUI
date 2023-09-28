using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers
{
    /// <summary>
    /// Rainfall runoff boundary conditions importer via BoundaryConditions.bc file.
    /// </summary>
    public class RRBoundaryConditionsBcImporter
    {
        /// <summary>
        /// Import of the data from the <paramref name="filePath"/> into the <paramref name="rainfallRunoffModel"/>.
        /// </summary>
        /// <param name="filePath">Location of the boundary conditions .bc file.</param>
        /// <param name="rainfallRunoffModel">Model in which the data of the boundary conditions .bc file is imported in.</param>
        /// <exception cref="ArgumentNullException">
        /// When <paramref name="filePath"/> or <paramref name="rainfallRunoffModel"/> is
        /// <c>null</c>.
        /// </exception>
        public void Import(string filePath, RainfallRunoffModel rainfallRunoffModel)
        {
            Ensure.NotNull(filePath, nameof(filePath));
            Ensure.NotNull(rainfallRunoffModel, nameof(rainfallRunoffModel));

            var logHandler = new LogHandler($"importing boundary conditions from the {Path.GetFileName(filePath)}");
            var parserProvider = new RRBoundaryConditionsDataParserProvider(logHandler, new BcSectionParser(logHandler));
            var converter = new RRBoundaryConditionsConverter(parserProvider);
            var setter = new RRBoundaryConditionsSetter(logHandler);

            IEnumerable<BcBlockData> bcBlockDatas = ReadBcFile(filePath);
            IReadOnlyDictionary<string, RainfallRunoffBoundaryData> data = converter.Convert(bcBlockDatas);
            SetLinkedUnpavedCatchmentBoundaryConditions(rainfallRunoffModel, data, setter);
            logHandler.LogReport();
        }

        private void SetLinkedUnpavedCatchmentBoundaryConditions(RainfallRunoffModel rainfallRunoffModel, IReadOnlyDictionary<string, RainfallRunoffBoundaryData> data, RRBoundaryConditionsSetter setter)
        {
            var rrBoundarySetterDataBlock = new RRModelBoundarySetterData(rainfallRunoffModel, data);
            setter.Set(rrBoundarySetterDataBlock);
        }

        private static IEnumerable<BcBlockData> ReadBcFile(string filePath)
        {
            var bcFileReader = new BcFile() { BlockKey = $"[{BoundaryRegion.BcBoundaryHeader}]" };
            IEnumerable<BcBlockData> bcBlockDatas = bcFileReader.Read(filePath);
            return bcBlockDatas;
        }
    }
}