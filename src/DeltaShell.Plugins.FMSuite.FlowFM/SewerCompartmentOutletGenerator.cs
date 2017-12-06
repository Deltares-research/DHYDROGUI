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

            if(gwswElement.IsValidGwswCompartment()) return CreateCompartmentForManhole<OutletCompartment>(gwswElement, network);

            var newOutlet = CreateOutletFromGwswStructureElement(gwswElement, network);
            
            //Get the parentmanhole and add the new outlet.
            var parentManhole = GetNewOrExistingManholeFromGwswElement(gwswElement, network);
            if (!parentManhole.ContainsCompartment(newOutlet.Name))
            {
                parentManhole.Compartments.Add(newOutlet);
            }

            return parentManhole;
        }

        private Compartment CreateOutletFromGwswStructureElement(GwswElement gwswElement, IHydroNetwork network)
        {
            var outletCompartment = FindOrGetNewCompartment<OutletCompartment>(gwswElement, network);
            ExtendOutletAttributes(outletCompartment, gwswElement);

            return outletCompartment;
        }

        private static void ExtendOutletAttributes(Compartment compartment, GwswElement gwswElement)
        {
            var newOutlet = compartment as OutletCompartment;
            if (newOutlet == null) return;

            var auxDouble = 0.0;

            var surfaceWaterLevel = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.SurfaceWaterLevel);
            if( surfaceWaterLevel.TryGetValueAsDouble(out auxDouble))
                newOutlet.SurfaceWaterLevel = auxDouble;
        }
    }
}