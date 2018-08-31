using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    class EggCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var eggShape = CreateEggShapeFromGwsw(gwswElement);
            return eggShape;
        }

        private ISewerFeature CreateEggShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                var csEggShape = new CrossSectionStandardShapeEgg
                {
                    Name = shapeName,
                    Width = width / 1000 /*Conversion from millimeters to meters*/
                };
                LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 1.5, "(2:3)", csEggShape);
                return csEggShape;
            }

            MessageForMissingValues(gwswElement, "width");
            return GetDefaultEggShape(shapeName);
        }

        private static ISewerFeature GetDefaultEggShape(string name)
        {
            var defaultTrapezoid = CrossSectionStandardShapeEgg.CreateDefault();
            defaultTrapezoid.Name = name;
            return defaultTrapezoid;
        }
    }
}