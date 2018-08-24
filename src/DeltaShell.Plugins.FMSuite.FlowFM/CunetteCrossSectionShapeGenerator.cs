using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class CunetteCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var cunetteShape = CreateCunetteShapeFromGwsw(gwswElement);
            return cunetteShape;
        }

        private ISewerFeature CreateCunetteShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                var csCunetteShape = new CrossSectionStandardShapeCunette
                {
                    Name = shapeName,
                    Width = width / 1000 /*Conversion from millimeters to meters*/
                };
                LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 0.634, "(1:0.634)", csCunetteShape);
                return csCunetteShape;
            }

            MessageForMissingValues(gwswElement, "width");
            return GetDefaultCunetteShape(shapeName);
        }

        private static ISewerFeature GetDefaultCunetteShape(string name)
        {
            var defaultTrapezoid = CrossSectionStandardShapeCunette.CreateDefault();
            defaultTrapezoid.Name = name;
            return defaultTrapezoid;
        }
    }
}