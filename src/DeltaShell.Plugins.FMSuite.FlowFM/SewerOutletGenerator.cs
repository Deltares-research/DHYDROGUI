using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerOutletGenerator: SewerFeatureFactory, ISewerNetworkFeatureGenerator
    {
        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            var outletStructure = SewerCompartmentGenerator.FindOrGetNewCompartment<OutletCompartment>(gwswElement, network);
            ExtendOutletAttributes(outletStructure, gwswElement);

            //If the network does not contain the manhole then it means it is a placeholder, but we still need to add it.
            if (network != null && !network.Manholes.Contains(outletStructure.ParentManhole))
            {
                network.Nodes.Add(outletStructure.ParentManhole);
            }

            return outletStructure;
        }

        private static void ExtendOutletAttributes(Compartment compartment, GwswElement gwswElement)
        {
            var newOutlet = compartment as OutletCompartment;
            if (newOutlet == null) return;

            var newDoubleValue = 0.0;
            var surfaceWaterLevel = SewerFeatureFactory.GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.SurfaceWaterLevel);
            if (surfaceWaterLevel != null && surfaceWaterLevel.ValueAsString != string.Empty)
            {
                var valueType = surfaceWaterLevel.GwswAttributeType.AttributeType;
                if (valueType == newOutlet.SurfaceWaterLevel.GetType() &&
                    TryParseDoubleElseLogError(surfaceWaterLevel, valueType, out newDoubleValue))
                {
                    newOutlet.SurfaceWaterLevel = newDoubleValue;
                }
            }
        }
    }
}