using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public interface ISewerFeatureGenerator
    {
        ISewerFeature Generate(GwswElement gwswElement);
    }
}