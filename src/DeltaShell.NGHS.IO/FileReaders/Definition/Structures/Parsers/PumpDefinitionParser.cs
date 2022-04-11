using System;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Parser for pumps.
    /// </summary>
    public class PumpDefinitionParser : StructureParserBase
    {
        /// <summary>
        /// Initializes a new <see cref="PumpDefinitionParser"/>.
        /// </summary>
        /// <param name="structureType">The structure type.</param>
        /// <param name="category">The <see cref="IDelftIniCategory"/> to parse a structure from.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFilename">The structures filename.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public PumpDefinitionParser(StructureType structureType,
                                    IDelftIniCategory category, 
                                    IBranch branch, 
                                    string structuresFilename) 
            : base(structureType, category, branch, structuresFilename) {}

        protected override IStructure1D Parse()
        {
            var pump = new Pump
            {
                Name = Category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = Category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = Branch,
                Chainage = Branch.GetBranchSnappedChainage(Category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                DirectionIsPositive = Category.ReadProperty<string>(StructureRegion.Orientation.Key, true, "positive")?.ToLower() == "positive",
                ControlDirection = GetControlDirectionFromString(Category.ReadProperty<string>(StructureRegion.Direction.Key)),
                Capacity = Category.ReadProperty<double>(StructureRegion.Capacity.Key),
            };

            pump.StartSuction = Category.ReadProperty<double>(StructureRegion.StartLevelSuctionSide.Key, pump.ControlDirection == PumpControlDirection.DeliverySideControl);
            pump.StopSuction = Category.ReadProperty<double>(StructureRegion.StopLevelSuctionSide.Key, pump.ControlDirection == PumpControlDirection.DeliverySideControl);
            pump.StartDelivery = Category.ReadProperty<double>(StructureRegion.StartLevelDeliverySide.Key, pump.ControlDirection == PumpControlDirection.SuctionSideControl);
            pump.StopDelivery = Category.ReadProperty<double>(StructureRegion.StopLevelDeliverySide.Key, pump.ControlDirection == PumpControlDirection.SuctionSideControl);


            var numReductionLevels = Category.ReadProperty<int>(StructureRegion.ReductionFactorLevels.Key, true, 0);
            if (numReductionLevels > 0)
            {
                var headValues = Category.ReadProperty<string>(StructureRegion.Head.Key, true).ToDoubleArray();
                var reductionFactorValues =
                    Category.ReadProperty<string>(StructureRegion.ReductionFactor.Key, true).ToDoubleArray();

                pump.ReductionTable = pump.ReductionTable.CreateFunctionFromArrays(headValues, reductionFactorValues);
            }

            return pump;
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