using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class DefaultCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public DefaultCrossSectionShapeGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            MessageForDefaultProfile(gwswElement);
            return new CrossSectionStandardShapeCircle
            {
                Name = GetCrossSectionShapeName(gwswElement),
                Diameter = 0.1,
                MaterialName = SewerProfileMapping.SewerProfileMaterial.Concrete.GetDescription()
            };
        }
    }
}