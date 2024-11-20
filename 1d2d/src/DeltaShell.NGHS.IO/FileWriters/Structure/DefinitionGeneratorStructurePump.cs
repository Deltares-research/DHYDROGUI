using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructurePump : DefinitionGeneratorTimeSeriesStructure
    {
        private const int DEFAULT_NRSTAGES = 1;
        private const int DEFAULT_REDUCTION_FACTOR_LEVELS = 0;
        private const double DEFAULT_HEAD = 0.0;
        private const double DEFAULT_REDUCTION_FACTOR = 1.0;
        
        public DefinitionGeneratorStructurePump(IStructureFileNameGenerator structureFileNameGenerator) : base(structureFileNameGenerator) {}
        
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Pump);

            var pump = hydroObject as Pump;
            if (pump == null)
            {
                return IniSection;
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

            return IniSection;
        }

        private void AddOrientation(IPump pump) => 
            IniSection.AddPropertyWithOptionalComment(StructureRegion.Orientation.Key, 
                                    pump.DirectionIsPositive ? "positive" : "negative", 
                                    StructureRegion.Orientation.Description);

        private void AddDirection(IPump pump) =>
            IniSection.AddPropertyWithOptionalComment(StructureRegion.Direction.Key, 
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
            IniSection.AddProperty(StructureRegion.NrStages.Key, 
                                    DEFAULT_NRSTAGES, 
                                    StructureRegion.NrStages.Description);


        private void AddCapacity(IPump pump)
        {
            AddProperty(pump.CanBeTimedependent && pump.UseCapacityTimeSeries,
                        StructureRegion.Capacity.Key,
                        pump.Capacity,
                        StructureRegion.Capacity.Description,
                        StructureRegion.Capacity.Format);
        }

        private void AddSuctionSideLevels(IPump pump)
        {
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.StartLevelSuctionSide.Key, 
                                    pump.StartSuction, 
                                    StructureRegion.StartLevelSuctionSide.Description, 
                                    StructureRegion.StartLevelSuctionSide.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.StopLevelSuctionSide.Key, 
                                    pump.StopSuction, 
                                    StructureRegion.StopLevelSuctionSide.Description, 
                                    StructureRegion.StopLevelSuctionSide.Format);
        }

        private void AddDeliverySideLevels(IPump pump)
        {
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.StartLevelDeliverySide.Key, 
                                    pump.StartDelivery, 
                                    StructureRegion.StartLevelDeliverySide.Description, 
                                    StructureRegion.StartLevelDeliverySide.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.StopLevelDeliverySide.Key, 
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

                IniSection.AddProperty(StructureRegion.ReductionFactorLevels.Key, 
                                        head.Count, 
                                        StructureRegion.ReductionFactorLevels.Description);
                IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.Head.Key, 
                                        head, 
                                        StructureRegion.Head.Description, StructureRegion.Head.Format);
                IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.ReductionFactor.Key, 
                                        reductionFactor, 
                                        StructureRegion.ReductionFactor.Description, 
                                        StructureRegion.ReductionFactor.Format);
            }
            else
            {
                IniSection.AddProperty(StructureRegion.ReductionFactorLevels.Key, 
                                        DEFAULT_REDUCTION_FACTOR_LEVELS, 
                                        StructureRegion.ReductionFactorLevels.Description);
                IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Head.Key, 
                                        DEFAULT_HEAD, 
                                        StructureRegion.Head.Description);
                IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.ReductionFactor.Key, 
                                        DEFAULT_REDUCTION_FACTOR, 
                                        StructureRegion.ReductionFactor.Description);
            }
        }
    }
}