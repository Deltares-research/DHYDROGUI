using System;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerCompartmentGenerator : ASewerCompartmentGenerator
    {
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