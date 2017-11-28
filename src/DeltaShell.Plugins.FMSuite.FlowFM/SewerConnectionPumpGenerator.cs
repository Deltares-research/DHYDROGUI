using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerConnectionPumpGenerator : SewerConnectionGenerator, ISewerNetworkFeatureGenerator
    {
        public new INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            var sewerConnection = CreateSewerConnection<SewerConnection>(gwswElement, network);
            AddPumpAndAttributesToSewerConnection(sewerConnection, gwswElement);
            return sewerConnection;
        }

        private static IPump FindOrCreatePump(ISewerConnection connection)
        {
            var structureFound = connection.BranchFeatures.OfType<IPump>().FirstOrDefault(bf => bf.Name.Equals(connection.Name));
            return structureFound ?? new Pump(connection.Name);
        }

        public static void AddPumpAndAttributesToSewerConnection(ISewerConnection connection, GwswElement gwswElement)
        {
            //Add pump to structure
            var sewerPump = FindOrCreatePump(connection);

            //Add Attributes
            var flowDirection = SewerFeatureFactory.GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.FlowDirection);
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