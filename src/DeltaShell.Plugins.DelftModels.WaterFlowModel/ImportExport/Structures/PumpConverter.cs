using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    /// <summary>
    /// This class is responsible for converting <see cref="IDelftIniCategory"/> objects into <see cref="Pump"/> objects.
    /// </summary>
    /// <seealso cref="AStructureConverter" />
    public class PumpConverter : AStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Pump();
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var pump = structure as Pump;

            pump.Capacity = category.ReadProperty<double>(StructureRegion.Capacity.Key);
            SetSuctionAndDeliveryTriggerProperties(category, pump);
            SetDirectionProperties(category, pump);
            SetReductionTableValues(category, pump);
        }

        private static void SetSuctionAndDeliveryTriggerProperties(IDelftIniCategory category, IPump pump)
        {
            pump.StartSuction = category.ReadProperty<double>(StructureRegion.StartLevelSuctionSide.Key);
            pump.StopSuction = category.ReadProperty<double>(StructureRegion.StopLevelSuctionSide.Key);
            pump.StartDelivery = category.ReadProperty<double>(StructureRegion.StartLevelDeliverySide.Key);
            pump.StopDelivery = category.ReadProperty<double>(StructureRegion.StopLevelDeliverySide.Key);
        }

        private static void SetDirectionProperties(IDelftIniCategory category, IPump pump)
        {
            var direction = category.ReadProperty<int>(StructureRegion.Direction.Key);
            pump.ControlDirection = (PumpControlDirection) Math.Abs(direction);
            pump.DirectionIsPositive = direction > 0;
        }

        private static void SetReductionTableValues(IDelftIniCategory category, IPump pump)
        {
            var numberOfFunctionEntries = category.ReadProperty<int>(StructureRegion.ReductionFactorLevels.Key);
            if (numberOfFunctionEntries <= 0) return;

            var pumpHeadValues = TransformToDoubleArray(category.ReadProperty<string>(StructureRegion.Head.Key));
            var reductionFactorValues = TransformToDoubleArray(category.ReadProperty<string>(StructureRegion.ReductionFactor.Key));
            for (var i = 0; i < numberOfFunctionEntries; i++)
            {
                pump.ReductionTable[pumpHeadValues[i]] = reductionFactorValues[i];
            }
        }
    }
}
