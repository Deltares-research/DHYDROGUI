using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class CunetteCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public CunetteCrossSectionShapeGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        public override ISewerFeature Generate(GwswElement gwswElement) 
        {
            var cunetteShape = CreateCunetteShapeFromGwsw(gwswElement);
            return cunetteShape;
        }

        private ISewerFeature CreateCunetteShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth, logHandler);
            if (widthAttribute.TryGetValueAsDouble(logHandler, out width))
            {
                var csCunetteShape = new CrossSectionStandardShapeCunette
                {
                    Name = shapeName,
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    MaterialName = GetMaterialValue(gwswElement)
                };
                LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 0.634, "(1:0.634)", csCunetteShape);
                return csCunetteShape;
            }

            MessageForMissingValues(gwswElement, "width");
            return GetDefaultCunetteShape(shapeName);
        }

        private static ISewerFeature GetDefaultCunetteShape(string name)
        {
            var defaultCunette = CrossSectionStandardShapeCunette.CreateDefault();
            defaultCunette.Name = name;
            defaultCunette.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultCunette;
        }
    }
}