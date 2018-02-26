using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class RectangleCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;
            
            double height;
            double width;
            CrossSectionStandardShapeRectangle csRectangleShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var heightAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileHeight);
            if (widthAttribute.TryGetValueAsDouble(out width) && heightAttribute.TryGetValueAsDouble(out height))
            {
                csRectangleShape = new CrossSectionStandardShapeRectangle
                {
                    Width = width / 1000, /*Conversion from millimeters to meters*/
                    Height = height / 1000 /*Conversion from millimeters to meters*/
                };
            }
            else
            {
                csRectangleShape = CrossSectionStandardShapeRectangle.CreateDefault();
                MessageForMissingValues(gwswElement, "width and/or height");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csRectangleShape, network);
            return null;
        }
    }
}