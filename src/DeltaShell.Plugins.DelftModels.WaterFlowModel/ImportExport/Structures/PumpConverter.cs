using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

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

        protected override void SetStructurePropertiesFromCategory(IList<string> warningMessages)
        {
            if (!(Structure is Pump pump)) return;

            pump.Capacity = Category.ReadProperty<double>(StructureRegion.Capacity.Key);
            SetSuctionTriggerProperties(pump);
            SetDeliveryTriggerProperties(pump);
            SetDirectionProperties(pump, warningMessages);
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

        private static void SetDirectionProperties(IPump pump, ICollection<string> warningMessages)
        {
            var direction = Category.ReadProperty<int>(StructureRegion.Direction.Key);
            var absoluteDirectionValue = Math.Abs(direction);
            if (Enum.IsDefined(typeof(PumpControlDirection), absoluteDirectionValue))
            {
                pump.ControlDirection = (PumpControlDirection) absoluteDirectionValue;
                pump.DirectionIsPositive = direction > 0;
            }
            else
            {
                warningMessages.Add(GetInvalidDirectionValueWarningMessage(pump, direction));
                pump.ControlDirection = PumpControlDirection.SuctionSideControl;
                pump.DirectionIsPositive = true;
            }
        }

        private static string GetInvalidDirectionValueWarningMessage(IPump pump, int direction)
        {
            var directionProperty = Category.Properties.FirstOrDefault(p =>
                string.Equals(p.Name, StructureRegion.Direction.Key, StringComparison.OrdinalIgnoreCase));
            var message = string.Format(Resources.PumpConverter_GetInvalidDirectionValueWarningMessage_Line__0___the_specified_value___1___for___2___is_invalid_,
                directionProperty?.LineNumber, direction, StructureRegion.Direction.Key, PumpControlDirection.SuctionSideControl, pump.Name);

            return message;
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
                var headProperty = Category.Properties.FirstOrDefault(p =>
                    string.Equals(p.Name, StructureRegion.Head.Key, StringComparison.OrdinalIgnoreCase));
                if (headProperty != null)
                {
                    var warningMessage = $"Line {headProperty.LineNumber}: The amount of defined head values for pump '{pumpName}' is not equal to the defined number at {StructureRegion.ReductionFactorLevels.Key}. The pump was not imported.";
                    warningMessages.Add(warningMessage);
                }
            }

            if (amountReductionFactorValues != numberOfFunctionEntries)
            {
                var reductionFactorProperty = Category.Properties.FirstOrDefault(p =>
                    string.Equals(p.Name, StructureRegion.ReductionFactor.Key, StringComparison.OrdinalIgnoreCase));
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
