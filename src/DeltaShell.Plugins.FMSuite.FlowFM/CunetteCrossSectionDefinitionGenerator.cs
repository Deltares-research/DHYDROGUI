using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class CunetteCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;

            double width;
            CrossSectionStandardShapeCunette csCunetteShape;
            var widthAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileWidth);
            if (widthAttribute.TryGetValueAsDouble(out width))
            {
                csCunetteShape = new CrossSectionStandardShapeCunette { Width = width / 1000 /*Conversion from millimeters to meters*/};
                LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(gwswElement, width, 0.634, "(1:0.634)", csCunetteShape);
            }
            else
            {
                csCunetteShape = CrossSectionStandardShapeCunette.CreateDefault();
                MessageForMissingValues(gwswElement, "width");
            }
            AddCrossSectionDefinitionToNetwork(gwswElement, csCunetteShape, network);
            return null;
        }
    }
}