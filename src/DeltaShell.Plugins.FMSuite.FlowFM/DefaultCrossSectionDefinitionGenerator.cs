using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Reflection;
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
                Diameter = 0.4,
                MaterialName = SewerProfileMapping.SewerProfileMaterial.Concrete.GetDescription()
            };
        }
    }
}