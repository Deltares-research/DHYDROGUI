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
    /// <seealso cref="StructureConverter" />
    public class PumpConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Pump();
        }

        protected override void SetStructureProperties()
        {
            var pump = Structure as Pump;

            pump.Capacity = Category.ReadProperty<double>(StructureRegion.Capacity.Key);
            SetSuctionAndDeliveryTriggerProperties(pump);
            SetDirectionProperties(pump);
            SetReductionTableValues(pump);
        }

        private static void SetSuctionAndDeliveryTriggerProperties(IPump pump)
        {
            pump.StartSuction = Category.ReadProperty<double>(StructureRegion.StartLevelSuctionSide.Key);
            pump.StopSuction = Category.ReadProperty<double>(StructureRegion.StopLevelSuctionSide.Key);
            pump.StartDelivery = Category.ReadProperty<double>(StructureRegion.StartLevelDeliverySide.Key);
            pump.StopDelivery = Category.ReadProperty<double>(StructureRegion.StopLevelDeliverySide.Key);
        }

        private static void SetDirectionProperties(IPump pump)
        {
            var direction = Category.ReadProperty<int>(StructureRegion.Direction.Key);
            pump.ControlDirection = (PumpControlDirection) Math.Abs(direction);
            pump.DirectionIsPositive = direction > 0;
        }

        private static void SetReductionTableValues(IPump pump)
        {
            var numberOfFunctionEntries = Category.ReadProperty<int>(StructureRegion.ReductionFactorLevels.Key);
            if (numberOfFunctionEntries <= 0) return;

            var pumpHeadValues = TransformToDoubleArray(Category.ReadProperty<string>(StructureRegion.Head.Key));
            var reductionFactorValues = TransformToDoubleArray(Category.ReadProperty<string>(StructureRegion.ReductionFactor.Key));
            for (var i = 0; i < numberOfFunctionEntries; i++)
            {
                pump.ReductionTable[pumpHeadValues[i]] = reductionFactorValues[i];
            }
        }
    }
}
