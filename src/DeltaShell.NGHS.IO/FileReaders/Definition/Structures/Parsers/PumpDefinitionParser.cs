using System;
using System.ComponentModel;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Parser for pumps.
    /// </summary>
    public class PumpDefinitionParser : StructureParserBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PumpDefinitionParser));

        private readonly ITimeSeriesFileReader fileReader;
        private readonly string structuresFilePath;
        private readonly DateTime referenceDateTime;

        /// <summary>
        /// Initializes a new <see cref="PumpDefinitionParser"/>.
        /// </summary>
        /// <param name="fileReader">The file reader</param>
        /// <param name="structureType">The structure type.</param>
        /// <param name="iniSection">The <see cref="IniSection"/> to parse a structure from.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFilePath">The structures file path.</param>
        /// <param name="referenceDateTime">The reference time date.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public PumpDefinitionParser(ITimeSeriesFileReader fileReader,
                                    StructureType structureType,
                                    IniSection iniSection,
                                    IBranch branch,
                                    string structuresFilePath,
                                    DateTime referenceDateTime)
            : base(structureType, iniSection, branch, Path.GetFileName(structuresFilePath))
        {
            Ensure.NotNull(fileReader, nameof(fileReader));
            this.fileReader = fileReader;

            this.structuresFilePath = structuresFilePath;
            this.referenceDateTime = referenceDateTime;
        }

        protected override IStructure1D Parse()
        {
            var pump = new Pump(true)
            {
                Name = IniSection.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = IniSection.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = Branch,
                Chainage = Branch.GetBranchSnappedChainage(IniSection.ReadProperty<double>(StructureRegion.Chainage.Key)),
                DirectionIsPositive = IniSection.ReadProperty<string>(StructureRegion.Orientation.Key, true, "positive")?.ToLower() == "positive",
                ControlDirection = GetControlDirectionFromString(IniSection.ReadProperty<string>(StructureRegion.Direction.Key)),
            };
            SetCapacity(pump);

            pump.StartSuction = IniSection.ReadProperty<double>(StructureRegion.StartLevelSuctionSide.Key, pump.ControlDirection == PumpControlDirection.DeliverySideControl);
            pump.StopSuction = IniSection.ReadProperty<double>(StructureRegion.StopLevelSuctionSide.Key, pump.ControlDirection == PumpControlDirection.DeliverySideControl);
            pump.StartDelivery = IniSection.ReadProperty<double>(StructureRegion.StartLevelDeliverySide.Key, pump.ControlDirection == PumpControlDirection.SuctionSideControl);
            pump.StopDelivery = IniSection.ReadProperty<double>(StructureRegion.StopLevelDeliverySide.Key, pump.ControlDirection == PumpControlDirection.SuctionSideControl);


            var numReductionLevels = IniSection.ReadProperty<int>(StructureRegion.ReductionFactorLevels.Key, true, 0);
            if (numReductionLevels > 0)
            {
                var headValues = IniSection.ReadProperty<string>(StructureRegion.Head.Key, true).ToDoubleArray();
                var reductionFactorValues =
                    IniSection.ReadProperty<string>(StructureRegion.ReductionFactor.Key, true).ToDoubleArray();

                pump.ReductionTable = pump.ReductionTable.CreateFunctionFromArrays(headValues, reductionFactorValues);
            }

            return pump;
        }

        private void SetCapacity(IPump pump)
        {
            var capacityValue = IniSection.ReadProperty<string>(StructureRegion.Capacity.Key);

            if (fileReader.IsTimeSeriesProperty(capacityValue))
            {
                ReadCapacityTimeSeries(pump, capacityValue);
            }
            else
            { 
                pump.Capacity = IniSection.ReadProperty<double>(StructureRegion.Capacity.Key);
            }
        }

        private void ReadCapacityTimeSeries(IPump pump, string relativeGateInitialOpeningPath)
        {
            string filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, relativeGateInitialOpeningPath);
            pump.UseCapacityTimeSeries = true;

            try
            {
                fileReader.Read(relativeGateInitialOpeningPath, filePath, new StructureTimeSeries(pump, pump.CapacityTimeSeries), referenceDateTime);
            }
            catch (FileReadingException e)
            {
                log.WarnFormat("Could not read the time series at {0} using default Capacity instead: {1}", filePath, e.Message);
                pump.UseCapacityTimeSeries = false;
            }
        }

        private static PumpControlDirection GetControlDirectionFromString(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "suctionside":  return PumpControlDirection.SuctionSideControl;
                case "deliveryside": return PumpControlDirection.DeliverySideControl;
                case "both":         return PumpControlDirection.SuctionAndDeliverySideControl;
                default:             return 0;
            }
        }
    }
}