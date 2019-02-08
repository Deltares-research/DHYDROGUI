using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override void SetStructurePropertiesFromCategory()
        {
            if (!(Structure is Pump pump)) return;

            pump.Capacity = Category.ReadProperty<double>(StructureRegion.Capacity.Key);
            SetSuctionTriggerProperties(pump);
            SetDeliveryTriggerProperties(pump);
            SetDirectionProperties(pump);
            SetReductionTableValues(pump);
        }

        private static void SetSuctionTriggerProperties(IPump pump)
        {
            pump.StartSuction = Category.ReadProperty<double>(StructureRegion.StartLevelSuctionSide.Key);
            pump.StopSuction = Category.ReadProperty<double>(StructureRegion.StopLevelSuctionSide.Key);
        }

        private static void SetDeliveryTriggerProperties(IPump pump)
        {
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
            ValidateReductionTableValues(numberOfFunctionEntries, pumpHeadValues.Length, reductionFactorValues.Length, pump.Name);

            for (var i = 0; i < numberOfFunctionEntries; i++)
            {
                pump.ReductionTable[pumpHeadValues[i]] = reductionFactorValues[i];
            }
        }

        private static void ValidateReductionTableValues(int numberOfFunctionEntries, int amountPumpHeadValues, int amountReductionFactorValues, string pumpName)
        {
            var warningMessages = new List<string>();
            if (amountPumpHeadValues != numberOfFunctionEntries)
            {
                var headProperty = Category.Properties.FirstOrDefault(p => p.Name == StructureRegion.Head.Key);
                if (headProperty != null)
                {
                    var warningMessage = $"Line {headProperty.LineNumber}: The amount of defined head values for pump '{pumpName}' is not equal to the defined number at {StructureRegion.ReductionFactorLevels.Key}. The pump was not imported.";
                    warningMessages.Add(warningMessage);
                }
            }

            if (amountReductionFactorValues != numberOfFunctionEntries)
            {
                var reductionFactorProperty = Category.Properties.FirstOrDefault(p => p.Name == StructureRegion.ReductionFactor.Key);
                if (reductionFactorProperty != null)
                {
                    var warningMessage = $"Line {reductionFactorProperty.LineNumber}: The amount of defined reduction factor values for pump '{pumpName}' is not equal to the defined number at {StructureRegion.ReductionFactorLevels.Key}. The pump was not imported.";
                    warningMessages.Add(warningMessage);
                }
            }

            if (warningMessages.Count > 0) throw new Exception(string.Join(Environment.NewLine, warningMessages));
        }
    }
}
