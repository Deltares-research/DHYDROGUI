using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    class PumpDefinitionReader : IStructureDefinitionReader
    {
        public IStructure1D ReadDefinition(IDelftIniCategory category,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var pump = new Pump
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                DirectionIsPositive = category.ReadProperty<string>(StructureRegion.Orientation.Key)?.ToLower() == "positive",
                ControlDirection = GetControlDirectionFromString(category.ReadProperty<string>(StructureRegion.Direction.Key)),
                Capacity = category.ReadProperty<double>(StructureRegion.Capacity.Key),
                StartSuction = category.ReadProperty<double>(StructureRegion.StartLevelSuctionSide.Key),
                StopSuction = category.ReadProperty<double>(StructureRegion.StopLevelSuctionSide.Key),
                StartDelivery = category.ReadProperty<double>(StructureRegion.StartLevelDeliverySide.Key),
                StopDelivery = category.ReadProperty<double>(StructureRegion.StopLevelDeliverySide.Key)
            };

            var useReductionTable = category.ReadProperty<bool>(StructureRegion.UseReductionTable.Key, true);
            if (useReductionTable)
            {
                var headValues = category.ReadProperty<string>(StructureRegion.Head.Key, true).ToDoubleArray();
                var reductionFactorValues =
                    category.ReadProperty<string>(StructureRegion.ReductionFactor.Key, true).ToDoubleArray();

                pump.ReductionTable = pump.ReductionTable.CreateFunctionFromArrays(headValues, reductionFactorValues);
            }

            return pump;
        }

        private static PumpControlDirection GetControlDirectionFromString(string value)
        {
            switch (value)
            {
                case "suctionSide":
                    return PumpControlDirection.SuctionSideControl;
                case "deliverySide":
                    return PumpControlDirection.DeliverySideControl;
                case "both":
                    return PumpControlDirection.SuctionAndDeliverySideControl;
                default:
                    return 0;
            }
        }
    }
}