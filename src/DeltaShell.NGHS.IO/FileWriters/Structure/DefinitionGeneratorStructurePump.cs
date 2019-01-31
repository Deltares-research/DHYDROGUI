using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructurePump : DefinitionGeneratorStructure
    {
        private const int DEFAULT_NRSTAGES = 1;
        private const int DEFAULT_REDUCTION_FACTOR_LEVELS = 0;
        private const double DEFAULT_HEAD = 0.0;
        private const double DEFAULT_REDUCTION_FACTOR = 1.0;

        public DefinitionGeneratorStructurePump(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure1D structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.Pump);

            if (structure is Pump pump)
            {
                AddDirectionProperties(pump);
                AddCommonPumpProperties(pump);
                AddReductionTableProperties(pump);
            }

            return IniCategory;
        }

        private void AddDirectionProperties(Pump pump)
        {
            var direction = pump.DirectionIsPositive
                ? (int) pump.ControlDirection
                : -1 * (int) pump.ControlDirection;
            IniCategory.AddProperty(StructureRegion.Direction.Key, direction, StructureRegion.Direction.Description);
        }

        private void AddCommonPumpProperties(IPump pump)
        {
            // Note: The computational core is expecting an array of doubles but there is only ever 1 value, see WFM1D.SetPump(...)
            IniCategory.AddProperty(StructureRegion.NrStages.Key, DEFAULT_NRSTAGES, StructureRegion.NrStages.Description);
            IniCategory.AddProperty(StructureRegion.Capacity.Key, pump.Capacity, StructureRegion.Capacity.Description,
                StructureRegion.Capacity.Format);
            IniCategory.AddProperty(StructureRegion.StartLevelSuctionSide.Key, pump.StartSuction,
                StructureRegion.StartLevelSuctionSide.Description, StructureRegion.StartLevelSuctionSide.Format);
            IniCategory.AddProperty(StructureRegion.StopLevelSuctionSide.Key, pump.StopSuction,
                StructureRegion.StopLevelSuctionSide.Description, StructureRegion.StopLevelSuctionSide.Format);
            IniCategory.AddProperty(StructureRegion.StartLevelDeliverySide.Key, pump.StartDelivery,
                StructureRegion.StartLevelDeliverySide.Description, StructureRegion.StartLevelDeliverySide.Format);
            IniCategory.AddProperty(StructureRegion.StopLevelDeliverySide.Key, pump.StopDelivery,
                StructureRegion.StopLevelDeliverySide.Description, StructureRegion.StopLevelDeliverySide.Format);
            // End note
        }

        private void AddReductionTableProperties(IPump pump)
        {
            if (pump.ReductionTable == null) return;

            var arguments = pump.ReductionTable.Arguments;
            var components = pump.ReductionTable.Components;

            if (arguments != null && components != null && arguments[0].Values.Count > 0 && components[0].Values.Count > 0)
            {
                var pumpHeadValues = arguments[0].Values.Cast<double>().ToList();
                var reductionFactorValues = components[0].Values.Cast<double>();

                IniCategory.AddProperty(StructureRegion.ReductionFactorLevels.Key, pumpHeadValues.Count,
                    StructureRegion.ReductionFactorLevels.Description);
                IniCategory.AddProperty(StructureRegion.Head.Key, pumpHeadValues, StructureRegion.Head.Description,
                    StructureRegion.Head.Format);
                IniCategory.AddProperty(StructureRegion.ReductionFactor.Key, reductionFactorValues,
                    StructureRegion.ReductionFactor.Description, StructureRegion.ReductionFactor.Format);
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.ReductionFactorLevels.Key, DEFAULT_REDUCTION_FACTOR_LEVELS,
                    StructureRegion.ReductionFactorLevels.Description);
                IniCategory.AddProperty(StructureRegion.Head.Key, DEFAULT_HEAD, StructureRegion.Head.Description);
                IniCategory.AddProperty(StructureRegion.ReductionFactor.Key, DEFAULT_REDUCTION_FACTOR,
                    StructureRegion.ReductionFactor.Description);
            }
        }
    }
}