using System.Collections.Generic;
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

        public DefinitionGeneratorStructurePump(KeyValuePair<int, string> compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.Pump);

            var pump = structure as Pump;
            if (pump == null) return IniCategory;

            var direction = (pump.DirectionIsPositive ? (int)pump.ControlDirection : -1 * (int)pump.ControlDirection);
            IniCategory.AddProperty(StructureRegion.Direction.Key, direction, StructureRegion.Direction.Description);

            // Note: The core is expecting an array of doubles but there is only ever 1 value, see WFM1D.SetPump(...)
            IniCategory.AddProperty(StructureRegion.NrStages.Key, DEFAULT_NRSTAGES, StructureRegion.NrStages.Description);
            IniCategory.AddProperty(StructureRegion.Capacity.Key, pump.Capacity, StructureRegion.Capacity.Description, StructureRegion.Capacity.Format);
            IniCategory.AddProperty(StructureRegion.StartLevelSuctionSide.Key, pump.StartSuction, StructureRegion.StartLevelSuctionSide.Description, StructureRegion.StartLevelSuctionSide.Format);
            IniCategory.AddProperty(StructureRegion.StopLevelSuctionSide.Key, pump.StopSuction, StructureRegion.StopLevelSuctionSide.Description, StructureRegion.StopLevelSuctionSide.Format);
            IniCategory.AddProperty(StructureRegion.StartLevelDeliverySide.Key, pump.StartDelivery, StructureRegion.StartLevelDeliverySide.Description, StructureRegion.StartLevelDeliverySide.Format);
            IniCategory.AddProperty(StructureRegion.StopLevelDeliverySide.Key, pump.StopDelivery, StructureRegion.StopLevelDeliverySide.Description, StructureRegion.StopLevelDeliverySide.Format);
            // End note

            if (pump.ReductionTable == null) return IniCategory;

            var arguments = pump.ReductionTable.Arguments;
            var components = pump.ReductionTable.Components;

            if (arguments != null && components != null && arguments[0].Values.Count > 0 && components[0].Values.Count > 0)
            {
                var head = arguments[0].Values.Cast<double>().ToList();
                var reductionFactor = components[0].Values.Cast<double>();

                IniCategory.AddProperty(StructureRegion.ReductionFactorLevels.Key, head.Count, StructureRegion.ReductionFactorLevels.Description);
                IniCategory.AddProperty(StructureRegion.Head.Key, head, StructureRegion.Head.Description, StructureRegion.Head.Format);
                IniCategory.AddProperty(StructureRegion.ReductionFactor.Key, reductionFactor, StructureRegion.ReductionFactor.Description, StructureRegion.ReductionFactor.Format);

            }
            else
            {
                IniCategory.AddProperty(StructureRegion.ReductionFactorLevels.Key, DEFAULT_REDUCTION_FACTOR_LEVELS, StructureRegion.ReductionFactorLevels.Description);
                IniCategory.AddProperty(StructureRegion.Head.Key, DEFAULT_HEAD, StructureRegion.Head.Description);
                IniCategory.AddProperty(StructureRegion.ReductionFactor.Key, DEFAULT_REDUCTION_FACTOR, StructureRegion.ReductionFactor.Description);
            }

            return IniCategory;
        }

    }
}