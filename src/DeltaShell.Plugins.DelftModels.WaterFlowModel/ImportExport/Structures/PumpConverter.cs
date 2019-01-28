using System;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    /// <summary>
    /// This class is responsible for converting <see cref="IDelftIniCategory"/> objects into <see cref="Pump"/> objects.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures.IStructureConverter" />
    public class PumpConverter : IStructureConverter
    {
        /// <summary>
        /// Converts a <see cref="IDelftIniCategory"/> object into a <see cref="Pump"/> object.
        /// </summary>
        /// <param name="category">The data model for setting property values on the pump.</param>
        /// <param name="branch">The branch on which the pump should be added.</param>
        /// <returns>A <see cref="Pump"/> object with properties set from <paramref name="category"/>.</returns>
        public IStructure1D ConvertToStructure1D(IDelftIniCategory category, IBranch branch)
        {
            var pump = new Pump();
            BasicStructuresOperations.ReadCommonRegionElements(category, branch, pump);

            pump.Capacity = category.ReadProperty<double>(StructureRegion.Capacity.Key);
            SetSuctionAndDeliveryTriggerPropertiesSetSuctionAndDeliveryTiggerProperties(category, pump);
            SetDirectionProperties(category, pump);
            SetReductionTableValues(category, pump);

            return pump;
        }

        private static void SetSuctionAndDeliveryTriggerPropertiesSetSuctionAndDeliveryTiggerProperties(IDelftIniCategory category, IPump pump)
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

        private static double[] TransformToDoubleArray(string valuesString)
        {
            return valuesString.Split(' ').Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray();
        }
    }
}
