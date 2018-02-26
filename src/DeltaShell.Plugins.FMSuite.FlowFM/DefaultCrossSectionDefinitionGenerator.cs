using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    class DefaultCrossSectionDefinitionGenerator : ASewerCrossSectionDefinitionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;
            MessageForDefaultProfile(gwswElement);
            var csRoundShape = new CrossSectionStandardShapeRound
            {
                Diameter = 0.4
            };
            AddCrossSectionDefinitionToNetwork(gwswElement, csRoundShape, network);
            return null;
        }
    }
}