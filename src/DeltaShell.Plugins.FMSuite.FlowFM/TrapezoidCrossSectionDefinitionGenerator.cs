using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class TrapezoidCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;
            
            double width;
            double height;
            double slope1;
            double slope2;
            double slope;
            CrossSectionStandardShapeTrapezium csTrapezoidShape;

            var slope1Attribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.Slope1);
            var slope2Attribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.Slope2);
            var slope1PresentAndWellFormatted = slope1Attribute.TryGetValueAsDouble(out slope1);
            var slope2PresentAndWellFormatted = slope2Attribute.TryGetValueAsDouble(out slope2);
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
                MessageForMissingValues(gwswElement, "width, height and/or slope");
                csTrapezoidShape = CrossSectionStandardShapeTrapezium.CreateDefault();
                AddCrossSectionDefinitionToNetwork(gwswElement, csTrapezoidShape, network);
                return null;
            }

            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (widthAttribute.TryGetValueAsDouble(out width) && heightAttribute.TryGetValueAsDouble(out height))
            {
                var trapezoidWidth = width / 1000;
                csTrapezoidShape = new CrossSectionStandardShapeTrapezium
                {
                    BottomWidthB = trapezoidWidth,
                    Slope = slope,
                    MaximumFlowWidth = (width + 2 * height / slope) / 1000
                };
            }
            else
            {
                csTrapezoidShape = CrossSectionStandardShapeTrapezium.CreateDefault();
                MessageForMissingValues(gwswElement, "width, height and/or slope");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csTrapezoidShape, network);
            return null;
        }
    }
}