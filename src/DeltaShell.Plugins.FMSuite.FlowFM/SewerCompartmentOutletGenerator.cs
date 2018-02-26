using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerCompartmentOutletGenerator : SewerCompartmentGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            return CreateCompartmentForManhole<OutletCompartment>(gwswElement, network);
        }

        protected override void SetCompartmentAttributes(Compartment compartment, GwswElement gwswElement)
        {
            var newOutlet = compartment as OutletCompartment;
            if (newOutlet == null) return;

            base.SetCompartmentAttributes(compartment, gwswElement);
            
            if(!gwswElement.IsValidGwswStructure()) return;
            double auxDouble;
            var surfaceWaterLevel = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.SurfaceWaterLevel);
            if( surfaceWaterLevel.TryGetValueAsDouble(out auxDouble))
                newOutlet.SurfaceWaterLevel = auxDouble;
        }
    }
}