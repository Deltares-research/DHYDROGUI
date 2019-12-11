using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    public class UShapeCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var UShapeShape = CreateUShapeShapeFromGwsw(gwswElement);
            return UShapeShape;
        }

        private ISewerFeature CreateUShapeShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double height;
            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (widthAttribute.TryGetValueAsDouble(out width) && heightAttribute.TryGetValueAsDouble(out height))
            {
                var arcHeight = height / 1000; /*Conversion from millimeters to meters*/
                return new CrossSectionStandardShapeUShape
                {
                    Name = shapeName,
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = arcHeight,
                    ArcHeight = arcHeight,
                    MaterialName = GetMaterialValue(gwswElement)
                };
            }

            MessageForMissingValues(gwswElement, "width and/or height");
            return GetDefaultUShapeShape(shapeName);
        }

        private static ISewerFeature GetDefaultUShapeShape(string name)
        {
            var defaultUShape = CrossSectionStandardShapeUShape.CreateDefault();
            defaultUShape.Name = name;
            defaultUShape.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultUShape;
        }
    }
}