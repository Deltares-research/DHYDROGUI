using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerPumpGenerator : ASewerGenerator, IGwswFeatureGenerator<ISewerFeature>
    { 
        public SewerPumpGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }

        public ISewerFeature Generate(GwswElement gwswElement)
        {
            var pump = CreateNewPump(gwswElement);
            return pump;
        }

        private IPump CreateNewPump(GwswElement gwswElement)
        {
            string pumpId = null;
            var uniqueIdKey = gwswElement.IsValidGwswSewerConnection(logHandler)
                ? SewerConnectionMapping.PropertyKeys.UniqueId
                : SewerStructureMapping.PropertyKeys.UniqueId;

            var pumpIdAttribute = gwswElement.GetAttributeFromList(uniqueIdKey, logHandler);
            if (pumpIdAttribute.IsValidAttribute(logHandler))
                pumpId = pumpIdAttribute.ValueAsString;

            if (gwswElement.IsValidGwswSewerConnection(logHandler))
            {
                var gwswConnectionPump = new GwswConnectionPump(pumpId);
                AddSewerConnectionAttributesToPump(gwswConnectionPump, gwswElement);
                return gwswConnectionPump;
            }

            var gwswStructurePump = new GwswStructurePump(pumpId);
            AddStructureAttributesToPump(gwswStructurePump, gwswElement);
            return gwswStructurePump;
        }

        private void AddSewerConnectionAttributesToPump(GwswConnectionPump pump, GwswElement gwswElement)
        {
            pump.SourceCompartmentName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, logHandler);
            pump.TargetCompartmentName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, logHandler);

            double auxDouble;
            var length = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Length, logHandler);
            if (length.TryGetValueAsDouble(logHandler, out auxDouble))
                pump.Length = auxDouble;

            var flowDirection = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.FlowDirection, logHandler);
            if (flowDirection.IsValidAttribute(logHandler))
            {
                var flowDirectionValue = flowDirection.GetValueFromDescription<SewerConnectionMapping.FlowDirection>(logHandler);
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
                        logHandler?.ReportError($"Cannot set pump flow direction with key {flowDirectionValue}");
                        break;
                }
            }
        }

        private void AddStructureAttributesToPump(IPump pump, GwswElement gwswElement)
        {
            double auxDouble;
            var pumpCapacity = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.PumpCapacity, logHandler);
            if (pumpCapacity.TryGetValueAsDouble(logHandler, out auxDouble))
                pump.Capacity = auxDouble / 3600; // File capacity is in m3/hour and in the GUI it is m3/s

            var startLevelDown = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StartLevelDownstreams, logHandler);
            if( startLevelDown.TryGetValueAsDouble(logHandler, out auxDouble))
                pump.StartSuction = auxDouble;

            var stopLevelDown = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StopLevelDownstreams, logHandler);
            if( stopLevelDown.TryGetValueAsDouble(logHandler, out auxDouble))
                pump.StopSuction = auxDouble;

            var startLevelUp = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StartLevelUpstreams, logHandler);
            if( startLevelUp.TryGetValueAsDouble(logHandler, out auxDouble))
                pump.StartDelivery = auxDouble;

            var stopLevelUp = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StopLevelUpstreams, logHandler);
            if (stopLevelUp.TryGetValueAsDouble(logHandler, out auxDouble))
                pump.StopDelivery = auxDouble;
        }
    }
}