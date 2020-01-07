using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.ImportExport.GWSW.Hydro.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.Gwsw
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