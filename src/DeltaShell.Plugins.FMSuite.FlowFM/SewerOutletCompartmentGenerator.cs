using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerOutletCompartmentGenerator : ASewerCompartmentGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            return gwswElement.IsValidGwswStructure() 
                ? CreateCompartment<GwswStructureOutletCompartment>(gwswElement) 
                : CreateCompartment<GwswPointOutletCompartment>(gwswElement);
        }
    }
}