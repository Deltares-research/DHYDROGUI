using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    class DefaultCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            MessageForDefaultProfile(gwswElement);
            return new CrossSectionStandardShapeCircle
            {
                Name = GetCrossSectionShapeName(gwswElement),
                Diameter = 0.4
            };
        }
    }
}