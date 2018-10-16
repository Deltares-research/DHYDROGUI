using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public class VisibilityVectorLayer : VectorLayer
    {
        public VisibilityVectorLayer():this(string.Empty)
        {
            
        }
            
        public VisibilityVectorLayer(string layername) : base(layername)
        {
        }
    }
}