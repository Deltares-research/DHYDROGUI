using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructurePump : DefinitionGeneratorStructure
    {
        private const int DEFAULT_NRSTAGES = 1;
        private const int DEFAULT_REDUCTION_FACTOR_LEVELS = 0;
        private const double DEFAULT_HEAD = 0.0;
        private const double DEFAULT_REDUCTION_FACTOR = 1.0;

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Pump);

            var pump = hydroObject as Pump;
            if (pump == null)
            {
                return IniCategory;
            }

            AddOrientation(pump);
            AddDirection(pump);

            // Note: The core is expecting an array of doubles but there is only ever 1 value, see WFM1D.SetPump(...)
            AddNrStages();
            AddCapacity(pump);
            AddSuctionSideLevels(pump);
            AddDeliverySideLevels(pump);
            // End note

            AddReductionTable(pump);

            return IniCategory;
        }

        private void AddOrientation(IPump pump) => 
            IniCategory.AddProperty(StructureRegion.Orientation.Key, 
                                    pump.DirectionIsPositive ? "positive" : "negative", 
                                    StructureRegion.Orientation.Description);

        private void AddDirection(IPump pump) =>
            IniCategory.AddProperty(StructureRegion.Direction.Key, 
                                    GetControlDirectionString(pump), 
                                    StructureRegion.Direction.Description);

        private static string GetControlDirectionString(IPump pump)
        {
            switch (pump.ControlDirection)
            {
                case PumpControlDirection.SuctionSideControl:
                    return "suctionSide";
                case PumpControlDirection.DeliverySideControl:
                    return "deliverySide";
                case PumpControlDirection.SuctionAndDeliverySideControl:
                    return "both";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddNrStages() => 
            IniCategory.AddProperty(StructureRegion.NrStages.Key, 
                                    DEFAULT_NRSTAGES, 
                                    StructureRegion.NrStages.Description);


        private void AddCapacity(IPump pump)
        {
            if (pump.CanBeTimedependent && pump.UseCapacityTimeSeries)
            {
                IniCategory.AddProperty(StructureRegion.Capacity.Key,
                                        StructureTimFileNameGenerator.Generate(pump, pump.CapacityTimeSeries), 
                                        StructureRegion.Capacity.Description);
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.Capacity.Key,
                                        pump.Capacity,
                                        StructureRegion.Capacity.Description,
                                        StructureRegion.Capacity.Format);
            }
        }

        private void AddSuctionSideLevels(IPump pump)
        {
            IniCategory.AddProperty(StructureRegion.StartLevelSuctionSide.Key, 
                                    pump.StartSuction, 
                                    StructureRegion.StartLevelSuctionSide.Description, 
                                    StructureRegion.StartLevelSuctionSide.Format);
            IniCategory.AddProperty(StructureRegion.StopLevelSuctionSide.Key, 
                                    pump.StopSuction, 
                                    StructureRegion.StopLevelSuctionSide.Description, 
                                    StructureRegion.StopLevelSuctionSide.Format);
        }

        private void AddDeliverySideLevels(IPump pump)
        {
            IniCategory.AddProperty(StructureRegion.StartLevelDeliverySide.Key, 
                                    pump.StartDelivery, 
                                    StructureRegion.StartLevelDeliverySide.Description, 
                                    StructureRegion.StartLevelDeliverySide.Format);
            IniCategory.AddProperty(StructureRegion.StopLevelDeliverySide.Key, 
                                    pump.StopDelivery, 
                                    StructureRegion.StopLevelDeliverySide.Description, 
                                    StructureRegion.StopLevelDeliverySide.Format);
        }

        private void AddReductionTable(IPump pump)
        {
            if (pump.ReductionTable == null) return;

            IEventedList<IVariable> arguments = pump.ReductionTable.Arguments;
            IEventedList<IVariable> components = pump.ReductionTable.Components;

            if (arguments != null && components != null && arguments[0].Values.Count > 0 && components[0].Values.Count > 0)
            {
                IList<double> head = arguments[0].Values.Cast<double>().ToList();
                IEnumerable<double> reductionFactor = components[0].Values.Cast<double>();

                IniCategory.AddProperty(StructureRegion.ReductionFactorLevels.Key, 
                                        head.Count, 
                                        StructureRegion.ReductionFactorLevels.Description);
                IniCategory.AddProperty(StructureRegion.Head.Key, 
                                        head, 
                                        StructureRegion.Head.Description, StructureRegion.Head.Format);
                IniCategory.AddProperty(StructureRegion.ReductionFactor.Key, 
                                        reductionFactor, 
                                        StructureRegion.ReductionFactor.Description, 
                                        StructureRegion.ReductionFactor.Format);
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.ReductionFactorLevels.Key, 
                                        DEFAULT_REDUCTION_FACTOR_LEVELS, 
                                        StructureRegion.ReductionFactorLevels.Description);
                IniCategory.AddProperty(StructureRegion.Head.Key, 
                                        DEFAULT_HEAD, 
                                        StructureRegion.Head.Description);
                IniCategory.AddProperty(StructureRegion.ReductionFactor.Key, 
                                        DEFAULT_REDUCTION_FACTOR, 
                                        StructureRegion.ReductionFactor.Description);
            }
        }
    }
}