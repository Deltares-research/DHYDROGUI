using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerPumpGenerator : ISewerNetworkFeatureGenerator
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerPumpGenerator));
        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement.IsValidGwswSewerConnection()) return CreatePumpFromGwswSewerConnection(gwswElement, network);
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
            var structureName = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.UniqueId);
            if (network == null)
            {
                Log.ErrorFormat(Resources.SewerPumpGenerator_CreatePumpFromGwswStructure_Pump_s__cannot_be_created_without_a_network_previously_defined_);
                return null;
            }
                
            if (!structureName.IsValidAttribute()) return null;


            var pumpFound = network.BranchFeatures.OfType<IPump>()
                .FirstOrDefault(p => p.Name.Equals(structureName.ValueAsString));
            if (pumpFound == null)
            {
                pumpFound = new Pump(structureName.ValueAsString);
                //Create a sewer connection placeholder and add it to the network so that the structure is later added as well.
                var auxSewerConnection = new SewerConnection(structureName.ValueAsString) { Network = network };
                network.Branches.Add(auxSewerConnection);
                auxSewerConnection.AddStructureToBranch(pumpFound);
            }

            ExtendPumpAttributes(pumpFound, gwswElement);

            return pumpFound;
        }

        private static void ExtendPumpAttributes(IPump pump, GwswElement gwswElement)
        {
            var auxDouble = 0.0;
            //Add Attributes
            var pumpCapacity = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.PumpCapacity);
            if (pumpCapacity.TryGetValueAsDouble(out auxDouble))
                pump.Capacity = auxDouble;

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
            var flowDirection = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.FlowDirection);
            if (flowDirection != null && flowDirection.ValueAsString != string.Empty)
            {
                var directionValue = flowDirection.GetValueFromDescription<SewerConnectionMapping.FlowDirection>();
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
                connection.AddStructureToBranch(sewerPump);
        }
    }
}