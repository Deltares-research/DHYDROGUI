using DelftTools.Hydro;
using SharpMap.Editors;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors
{
    public abstract class DrainageBasinFeatureEditor : FeatureEditor
    {
        public IDrainageBasin DrainageBasin { get; set; }
    }
}