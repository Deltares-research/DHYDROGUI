using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerCompartmentOutletGenerator : SewerCompartmentGenerator, ISewerNetworkFeatureGenerator
    {
        public new INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement == null) return null;

            if(IsValidGwswCompartment(gwswElement)) return CreateCompartment<OutletCompartment>(gwswElement, network);
            
            return CreateOutletFromGwswStructureElement(gwswElement, network);
        }

        private INetworkFeature CreateOutletFromGwswStructureElement(GwswElement gwswElement, IHydroNetwork network)
        {
            var outletCompartment = FindOrGetNewCompartment<OutletCompartment>(gwswElement, network);
            ExtendOutletAttributes(outletCompartment, gwswElement);
            
            //If the network does not contain the manhole then it means it is a placeholder, but we still need to add it.
            if (network != null && !network.Nodes.Contains(outletCompartment.ParentManhole))
            {
                network.Nodes.Add(outletCompartment.ParentManhole);
            }

            return outletCompartment;
        }

        private static void ExtendOutletAttributes(Compartment compartment, GwswElement gwswElement)
        {
            var newOutlet = compartment as OutletCompartment;
            if (newOutlet == null) return;

            var newDoubleValue = 0.0;
            var surfaceWaterLevel = GetAttributeFromList(gwswElement, SewerStructureMapping.PropertyKeys.SurfaceWaterLevel);
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