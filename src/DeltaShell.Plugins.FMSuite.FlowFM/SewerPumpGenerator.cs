using System;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerPumpGenerator : ISewerFeatureGenerator
    {
        public ISewerFeature Generate(GwswElement gwswElement)
        {
            var pump = CreateNewPump(gwswElement);
            return pump;
        }

        private static IPump CreateNewPump(GwswElement gwswElement)
        {
            string pumpId = null;
            var uniqueIdKey = gwswElement.IsValidGwswSewerConnection()
                ? SewerConnectionMapping.PropertyKeys.UniqueId
                : SewerStructureMapping.PropertyKeys.UniqueId;

            var pumpIdAttribute = gwswElement.GetAttributeFromList(uniqueIdKey);
            if (pumpIdAttribute.IsValidAttribute())
                pumpId = pumpIdAttribute.ValueAsString;

            if (gwswElement.IsValidGwswSewerConnection())
            {
                var gwswConnectionPump = new GwswConnectionPump(pumpId);
                AddSewerConnectionAttributesToPump(gwswConnectionPump, gwswElement);
                return gwswConnectionPump;
            }

            var gwswStructurePump = new GwswStructurePump(pumpId);
            AddStructureAttributesToPump(gwswStructurePump, gwswElement);
            return gwswStructurePump;
        }

        private static void AddSewerConnectionAttributesToPump(GwswConnectionPump pump, GwswElement gwswElement)
        {
            var sourceCompartmentAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SourceCompartmentId);
            pump.SourceCompartmentName = sourceCompartmentAttribute.GetValidStringValue();

            var targetCompartmentAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.TargetCompartmentId);
            pump.TargetCompartmentName= targetCompartmentAttribute.GetValidStringValue();

            double auxDouble;
            var length = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Length);
            if (length.TryGetValueAsDouble(out auxDouble))
                pump.Length = auxDouble;

            var flowDirection = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.FlowDirection);
            if (flowDirection.IsValidAttribute())
            {
                var flowDirectionValue = flowDirection.GetValueFromDescription<SewerConnectionMapping.FlowDirection>();
                switch (flowDirectionValue)
                {
                    case SewerConnectionMapping.FlowDirection.Open:
                    case SewerConnectionMapping.FlowDirection.Closed:
                    case SewerConnectionMapping.FlowDirection.FromStartToEnd:
                        pump.DirectionIsPositive = true;
                        break;
                    case SewerConnectionMapping.FlowDirection.FromEndToStart:
                        pump.DirectionIsPositive = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void AddStructureAttributesToPump(IPump pump, GwswElement gwswElement)
        {
            double auxDouble;
            var pumpCapacity = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.PumpCapacity);
            if (pumpCapacity.TryGetValueAsDouble(out auxDouble))
                pump.Capacity = auxDouble / 3600; // File capacity is in m3/hour and in the GUI it is m3/s

            var startLevelDown = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StartLevelDownstreams);
            if( startLevelDown.TryGetValueAsDouble(out auxDouble))
                pump.StartSuction = auxDouble;

            var stopLevelDown = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StopLevelDownstreams);
            if( stopLevelDown.TryGetValueAsDouble(out auxDouble))
                pump.StopSuction = auxDouble;

            var startLevelUp = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StartLevelUpstreams);
            if( startLevelUp.TryGetValueAsDouble(out auxDouble))
                pump.StartDelivery = auxDouble;

            var stopLevelUp = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StopLevelUpstreams);
            if (stopLevelUp.TryGetValueAsDouble(out auxDouble))
                pump.StopDelivery = auxDouble;
        }
    }
}