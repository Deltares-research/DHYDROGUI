using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public interface ISewerNetworkFeatureGenerator
    {
        ISewerFeature Generate(GwswElement gwswElement);
    }
}