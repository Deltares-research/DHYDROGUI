using DelftTools.Hydro.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    public class SewerCompartmentGenerator : ASewerCompartmentGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            return gwswElement == null ? null : CreateCompartment<Compartment>(gwswElement);
        }
    }
}