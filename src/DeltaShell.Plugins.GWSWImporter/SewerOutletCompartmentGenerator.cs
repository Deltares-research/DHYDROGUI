using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerOutletCompartmentGenerator : ASewerCompartmentGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            bool validGwswStructure = gwswElement.IsValidGwswStructure();

            if (validGwswStructure)
            {
                return CreateCompartment<GwswStructureOutletCompartment>(gwswElement);
            }

            return CreateCompartment<GwswPointOutletCompartment>(gwswElement);
        }

        protected override void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswCompartment()) return;

            double auxDouble;
            var outletCompartment = compartment as OutletCompartment;
            if (outletCompartment != null)
            {
                var surfaceWaterLevelAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.SurfaceWaterLevel);
                if (surfaceWaterLevelAttribute.TryGetValueAsDouble(out auxDouble))
                    outletCompartment.SurfaceWaterLevel = auxDouble;
            }
            
            if (compartment is GwswStructureOutletCompartment)
            {
                compartment.ManholeLength = 0.8d;
                compartment.ManholeWidth = 0.8d;
                compartment.FloodableArea = 100;

                return;
            }

            SetBaseCompartmentProperties(compartment, gwswElement);
        }
    }
}