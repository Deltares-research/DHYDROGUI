using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerPumpGenerator : SewerFeatureFactory, ISewerNetworkFeatureGenerator
    {
        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if( network == null) //Log message we cannot process pump without network.
                return null;

            var structureName = SewerFeatureFactory.GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.UniqueId);
            if (structureName == null || structureName.ValueAsString == string.Empty) return null;

            
            var pumpFound = network.BranchFeatures.OfType<IPump>()
                .FirstOrDefault(p => p.Name.Equals(structureName.ValueAsString));
            if (pumpFound == null)
            {
                pumpFound = new Pump(structureName.ValueAsString);
                //Create a sewer connection placeholder and add it to the network so that the structure is later added as well.
                var auxSewerConnection = new SewerConnection(structureName.ValueAsString) { Network = network };
                network.Branches.Add(auxSewerConnection);
                AddStructureToBranch(auxSewerConnection, pumpFound);
            }
            ExtendPumpAttributes(pumpFound, gwswElement);

            return pumpFound;
            
            
        }

        private static void ExtendPumpAttributes(IPump pump, GwswElement gwswElement)
        {
            //Add Attributes
            var newDoubleValue = 0.0;
            var pumpCapacity = SewerFeatureFactory.GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.PumpCapacity);
            if (pumpCapacity != null && pumpCapacity.ValueAsString != string.Empty)
            {
                var valueType = pumpCapacity.GwswAttributeType.AttributeType;
                if (valueType == pump.Capacity.GetType() &&
                    TryParseDoubleElseLogError(pumpCapacity, valueType, out newDoubleValue))
                {
                    pump.Capacity = newDoubleValue;
                }
            }
            var startLevelDown = SewerFeatureFactory.GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StartLevelDownstreams);
            if (startLevelDown != null && startLevelDown.ValueAsString != string.Empty)
            {
                var valueType = startLevelDown.GwswAttributeType.AttributeType;
                if (valueType == pump.StartSuction.GetType() &&
                    TryParseDoubleElseLogError(startLevelDown, valueType, out newDoubleValue))
                {
                    pump.StartSuction = newDoubleValue;
                }
            }
            var stopLevelDown = SewerFeatureFactory.GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StopLevelDownstreams);
            if (stopLevelDown != null && stopLevelDown.ValueAsString != string.Empty)
            {
                var valueType = stopLevelDown.GwswAttributeType.AttributeType;
                if (valueType == pump.StopSuction.GetType() &&
                    TryParseDoubleElseLogError(stopLevelDown, valueType, out newDoubleValue))
                {
                    pump.StopSuction = newDoubleValue;
                }
            }

            var startLevelUp = SewerFeatureFactory.GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StartLevelUpstreams);
            if (startLevelUp != null && startLevelUp.ValueAsString != string.Empty)
            {
                var valueType = startLevelUp.GwswAttributeType.AttributeType;
                if (valueType == pump.StartDelivery.GetType() &&
                    TryParseDoubleElseLogError(startLevelUp, valueType, out newDoubleValue))
                {
                    pump.StartDelivery = newDoubleValue;
                }
            }
            var stopLevelUp = SewerFeatureFactory.GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StopLevelUpstreams);
            if (stopLevelUp != null && stopLevelUp.ValueAsString != string.Empty)
            {
                var valueType = stopLevelUp.GwswAttributeType.AttributeType;
                if (valueType == pump.StopDelivery.GetType() &&
                    TryParseDoubleElseLogError(stopLevelUp, valueType, out newDoubleValue))
                {
                    pump.StopDelivery = newDoubleValue;
                }
            }
        }
    }
}