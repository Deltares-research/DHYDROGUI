using DelftTools.Hydro.SewerFeatures;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerCompartmentGenerator : ASewerCompartmentGenerator
    {
        public SewerCompartmentGenerator(ILogHandler logHandler) : base(logHandler)
        {
        }
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            return gwswElement == null ? null : CreateCompartment<Compartment>(gwswElement);
        }

        protected override void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement)
        {
            SetBaseCompartmentProperties(compartment, gwswElement);
        }
    }
}