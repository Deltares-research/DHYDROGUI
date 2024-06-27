using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class TrapezoidCrossSectionShapeGenerator : ASewerCrossSectionShapeGenerator
    {
        public TrapezoidCrossSectionShapeGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        
        public override ISewerFeature Generate(GwswElement gwswElement)
        {
            var trapezoidShape = CreateTrapezoidShapeFromGwsw(gwswElement);
            return trapezoidShape;
        }

        private ISewerFeature CreateTrapezoidShapeFromGwsw(GwswElement gwswElement)
        {
            var shapeName = GetCrossSectionShapeName(gwswElement);

            double height;
            double width;
            double slope;
            var widthHeightAndOrSlope = "width, height and/or slope";

            double slope1;
            double slope2;

            var slope1Attribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.Slope1, logHandler);
            var slope2Attribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.Slope2, logHandler);
            var slope1PresentAndWellFormatted = slope1Attribute.TryGetValueAsDouble(logHandler, out slope1);
            var slope2PresentAndWellFormatted = slope2Attribute.TryGetValueAsDouble(logHandler, out slope2);
            if (slope1PresentAndWellFormatted && !slope2PresentAndWellFormatted)
            {
                slope = slope1;
            }
            else if (!slope1PresentAndWellFormatted && slope2PresentAndWellFormatted)
            {
                slope = slope2;
            }
            else if (slope1PresentAndWellFormatted)
            {
                slope = (slope1 + slope2) / 2;
            }
            else
            {
                MessageForMissingValues(gwswElement, widthHeightAndOrSlope);
                return GetDefaultTrapezoid(shapeName);
            }

            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth, logHandler);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight, logHandler);
            if (widthAttribute.TryGetValueAsDouble(logHandler, out width) && heightAttribute.TryGetValueAsDouble(logHandler, out height))
            {
                var trapezoidWidth = width / 1000;
                return new CrossSectionStandardShapeTrapezium
                {
                    Name = shapeName,
                    BottomWidthB = trapezoidWidth,
                    Slope = slope,
                    MaximumFlowWidth = (width + 2 * height / slope) / 1000,
                    MaterialName = GetMaterialValue(gwswElement)
                };
            }

            MessageForMissingValues(gwswElement, widthHeightAndOrSlope);
            return GetDefaultTrapezoid(shapeName);
        }

        private static ISewerFeature GetDefaultTrapezoid(string name)
        {
            var defaultTrapezoid = CrossSectionStandardShapeTrapezium.CreateDefault();
            defaultTrapezoid.Name = name;
            defaultTrapezoid.MaterialName = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
            return defaultTrapezoid;
        }

        
    }
}