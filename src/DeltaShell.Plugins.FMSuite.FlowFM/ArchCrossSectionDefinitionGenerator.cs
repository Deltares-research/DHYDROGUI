using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class ArchCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var archShape = CreateArchShapeFromGwsw(gwswElement);
            return archShape;
        }

        private ISewerFeature CreateArchShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double height;
            double width;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (widthAttribute.TryGetValueAsDouble(out width) && heightAttribute.TryGetValueAsDouble(out height))
            {
                var arcHeight = height / 1000; /*Conversion from millimeters to meters*/
                return new CrossSectionStandardShapeArch
                {
                    Name = shapeName,
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = arcHeight,
                    ArcHeight = arcHeight
                };
            }

            MessageForMissingValues(gwswElement, "width and/or height");
            return GetDefaultArchShape(shapeName);
        }

        private static ISewerFeature GetDefaultArchShape(string name)
        {
            var defaultArch = CrossSectionStandardShapeArch.CreateDefault();
            defaultArch.Name = name;
            return defaultArch;
        }
    }
}