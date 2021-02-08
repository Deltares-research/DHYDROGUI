using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerOutletCompartmentGenerator : ASewerCompartmentGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            return gwswElement.IsValidGwswStructure() 
                ? CreateCompartment<GwswStructureOutletCompartment>(gwswElement) 
                : CreateCompartment<GwswPointOutletCompartment>(gwswElement);
        }

        protected override void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswCompartment()) return;

            compartment.ManholeLength = 0.8d;
            compartment.ManholeWidth = 0.8d;
            compartment.FloodableArea = 100;
            
            double auxDouble;
            var outletCompartment = compartment as OutletCompartment;
            if (outletCompartment != null)
            {
                var surfaceWaterLevelAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.SurfaceWaterLevel);
                if (surfaceWaterLevelAttribute.TryGetValueAsDouble(out auxDouble))
                    outletCompartment.SurfaceWaterLevel = auxDouble;
            }
        }
    }
}