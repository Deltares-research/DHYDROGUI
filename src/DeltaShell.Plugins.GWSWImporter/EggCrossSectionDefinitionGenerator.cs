using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class EggCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
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
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    MaterialName = GetMaterialValue(gwswElement)
                };
                LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 1.5, "(2:3)", csEggShape);
                return csEggShape;
            }

            MessageForMissingValues(gwswElement, "width");
            return GetDefaultEggShape(shapeName);
        }

        private static ISewerFeature GetDefaultEggShape(string name)
        {
            var defaultEgg = CrossSectionStandardShapeEgg.CreateDefault();
            defaultEgg.Name = name;
            defaultEgg.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultEgg;
        }
    }
}