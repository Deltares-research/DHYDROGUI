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
            if (IsValidGwswSewerConnection(gwswElement)) return CreatePumpFromGwswSewerConnection(gwswElement, network);
            return CreatePumpFromGwswStructure(gwswElement, network);
        }

        private INetworkFeature CreatePumpFromGwswSewerConnection(GwswElement gwswElement, IHydroNetwork network)
        {
            var sewerConnection = (SewerConnection) new SewerConnectionGenerator().Generate(gwswElement, network);
            AddPumpAndAttributesToSewerConnection(sewerConnection, gwswElement);
            return sewerConnection;
        }

        private INetworkFeature CreatePumpFromGwswStructure(GwswElement gwswElement, IHydroNetwork network)
        {
            if (network == null) //Log message we cannot process pump (structure) without network.
                return null;

            var structureName = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.UniqueId);
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
            var pumpCapacity = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.PumpCapacity);
            if (pumpCapacity != null && pumpCapacity.ValueAsString != string.Empty)
            {
                var valueType = pumpCapacity.GwswAttributeType.AttributeType;
                if (valueType == pump.Capacity.GetType() &&
                    TryParseDoubleElseLogError(pumpCapacity, valueType, out newDoubleValue))
                {
                    pump.Capacity = newDoubleValue;
                }
            }
            var startLevelDown = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StartLevelDownstreams);
            if (startLevelDown != null && startLevelDown.ValueAsString != string.Empty)
            {
                var valueType = startLevelDown.GwswAttributeType.AttributeType;
                if (valueType == pump.StartSuction.GetType() &&
                    TryParseDoubleElseLogError(startLevelDown, valueType, out newDoubleValue))
                {
                    pump.StartSuction = newDoubleValue;
                }
            }
            var stopLevelDown = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StopLevelDownstreams);
            if (stopLevelDown != null && stopLevelDown.ValueAsString != string.Empty)
            {
                var valueType = stopLevelDown.GwswAttributeType.AttributeType;
                if (valueType == pump.StopSuction.GetType() &&
                    TryParseDoubleElseLogError(stopLevelDown, valueType, out newDoubleValue))
                {
                    pump.StopSuction = newDoubleValue;
                }
            }

            var startLevelUp = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StartLevelUpstreams);
            if (startLevelUp != null && startLevelUp.ValueAsString != string.Empty)
            {
                var valueType = startLevelUp.GwswAttributeType.AttributeType;
                if (valueType == pump.StartDelivery.GetType() &&
                    TryParseDoubleElseLogError(startLevelUp, valueType, out newDoubleValue))
                {
                    pump.StartDelivery = newDoubleValue;
                }
            }
            var stopLevelUp = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StopLevelUpstreams);
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

        private static IPump FindOrCreatePump(ISewerConnection connection)
        {
            var structureFound = connection.BranchFeatures.OfType<IPump>().FirstOrDefault(bf => bf.Name.Equals(connection.Name));
            return structureFound ?? new Pump(connection.Name);
        }

        private static void AddPumpAndAttributesToSewerConnection(ISewerConnection connection, GwswElement gwswElement)
        {
            //Add pump to structure
            var sewerPump = FindOrCreatePump(connection);

            //Add Attributes
            var flowDirection = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.FlowDirection);
            if (flowDirection != null && flowDirection.ValueAsString != string.Empty)
            {
                var directionValue = GetValueFromDescription<SewerConnectionMapping.FlowDirection>(flowDirection.ValueAsString);
                if (directionValue == SewerConnectionMapping.FlowDirection.FromStartToEnd)
                {
                    sewerPump.DirectionIsPositive = true;
                }
                if (directionValue == SewerConnectionMapping.FlowDirection.FromEndToStart)
                {
                    sewerPump.DirectionIsPositive = false;
                }
            }

            //Add pump to network if it´s not present already
            if (!connection.BranchFeatures.Contains(sewerPump))
                AddStructureToBranch(connection, sewerPump);
        }
    }
}