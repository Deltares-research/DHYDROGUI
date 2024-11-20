using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    class InvertedEggCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public InvertedEggCrossSectionShapeGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var invertedEggShape = CreateInvertedEggShapeFromGwsw(gwswElement);
            return invertedEggShape;
        }

        private ISewerFeature CreateInvertedEggShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth, logHandler);
            if (widthAttribute.TryGetValueAsDouble(logHandler, out width))
            {
                var csInvertedEggShape = new CrossSectionStandardShapeInvertedEgg
                {
                    Name = shapeName,
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    MaterialName = GetMaterialValue(gwswElement)
                };
                LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 1.5, "(2:3)", csInvertedEggShape);
                return csInvertedEggShape;
            }

            MessageForMissingValues(gwswElement, "width");
            return GetDefaultInvertedEggShape(shapeName);
        }

        private static ISewerFeature GetDefaultInvertedEggShape(string name)
        {
            var defaultInvertedEgg = CrossSectionStandardShapeInvertedEgg.CreateDefault();
            defaultInvertedEgg.Name = name;
            defaultInvertedEgg.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultInvertedEgg;
        }

        
    }
}