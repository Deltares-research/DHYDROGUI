using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers
{
    [Entity(FireOnCollectionChange = false)]
    public class HydroRegionMapLayer : GroupLayer
    {
        [Aggregation]
        public IHydroRegion Region { get; set; }
    }
}