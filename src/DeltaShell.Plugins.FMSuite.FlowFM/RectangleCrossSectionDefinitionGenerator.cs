using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class RectangleCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var rectangleShape = CreateRectangleShapeFromGwsw(gwswElement);
            return rectangleShape;
        }

        private ISewerFeature CreateRectangleShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double height;
            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (widthAttribute.TryGetValueAsDouble(out width) && heightAttribute.TryGetValueAsDouble(out height))
            {
                return new CrossSectionStandardShapeRectangle
                {
                    Name = shapeName,
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = height / 1000, /*Conversion from millimeters to meters*/
                    MaterialName = GetMaterialValue(gwswElement)
                };
            }

            MessageForMissingValues(gwswElement, "width and/or height");
            return GetDefaultRectangleShape(shapeName);
        }

        private static ISewerFeature GetDefaultRectangleShape(string name)
        {
            var defaultRectangle = CrossSectionStandardShapeRectangle.CreateDefault();
            defaultRectangle.Name = name;
            defaultRectangle.MaterialName = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerProfileMapping.SewerProfileMaterial.Unknown);
            return defaultRectangle;
        }
    }
}