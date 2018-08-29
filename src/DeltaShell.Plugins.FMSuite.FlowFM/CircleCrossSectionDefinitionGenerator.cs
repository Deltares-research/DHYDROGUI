using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class CircleCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var roundShape = CreateRoundShapeFromGwsw(gwswElement);
            return roundShape;
        }

        private CrossSectionStandardShapeCircle CreateRoundShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var materialAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileMaterial);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                var multiplier = 1.0;
                var pvcStringId = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerProfileMapping.SewerProfileMaterial.Polyvinylchlorid);
                if (materialAttribute.IsValidAttribute() && materialAttribute.ValueAsString.Equals(pvcStringId)) multiplier = 16.0 / 17.0;
                return new CrossSectionStandardShapeCircle
                {
                    Name = shapeName,
                    Diameter = multiplier * width / 1000 /*Conversion from millimeters to meters*/
                };
            }

            MessageForMissingValues(gwswElement, "width");
            return GetDefaultRoundShape(shapeName);
        }

        private static CrossSectionStandardShapeCircle GetDefaultRoundShape(string name)
        {
            var defaultCircle = CrossSectionStandardShapeCircle.CreateDefault();
            defaultCircle.Name = name;
            return defaultCircle;
        }
    }
}