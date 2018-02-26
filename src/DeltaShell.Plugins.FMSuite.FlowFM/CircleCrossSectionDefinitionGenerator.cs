using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class CircleCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;
            
            double width;
            CrossSectionStandardShapeRound csRoundShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            var materialAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileMaterial);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                var multiplier = 1.0;
                var pvcStringId = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerProfileMapping.SewerProfileMaterial.Polyvinylchlorid);
                if (materialAttribute.IsValidAttribute() && materialAttribute.ValueAsString.Equals(pvcStringId)) multiplier = 16.0 / 17.0;
                csRoundShape = new CrossSectionStandardShapeRound
                {
                    Diameter = multiplier * width / 1000 /*Conversion from millimeters to meters*/
                };
            }
            else
            {
                csRoundShape = CrossSectionStandardShapeRound.CreateDefault();
                MessageForMissingValues(gwswElement, "width");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csRoundShape, network);
            return null;
        }
    }
}