using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public interface ISewerNetworkFeatureGenerator
    {
        INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null);
    }
}