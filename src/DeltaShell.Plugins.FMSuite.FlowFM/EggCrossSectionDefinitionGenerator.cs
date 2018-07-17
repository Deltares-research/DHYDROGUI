using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    class EggCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;

            double width;
            CrossSectionStandardShapeEgg csEggShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                csEggShape = new CrossSectionStandardShapeEgg { Width = width / 1000 /*Conversion from millimeters to meters*/};
                LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 1.5, "(2:3)", csEggShape);
            }
            else
            {
                csEggShape = CrossSectionStandardShapeEgg.CreateDefault();
                MessageForMissingValues(gwswElement, "width");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csEggShape, network);
            return null;
        }
    }
}