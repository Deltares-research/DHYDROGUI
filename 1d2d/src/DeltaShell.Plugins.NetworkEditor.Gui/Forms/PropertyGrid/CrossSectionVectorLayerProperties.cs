using System.Linq;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.GridProperties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class CrossSectionVectorLayerProperties : VectorLayerLineProperties
    {
        public bool UseDefaultSize
        {
            get { return CrossSectionRenderer.UseDefaultLength; }
            set
            {
                CrossSectionRenderer.UseDefaultLength = value;
                data.RenderRequired = true;
            }
        }

        public double DefaultSize
        {
            get { return CrossSectionRenderer.DefaultLength; }
            set
            {
                CrossSectionRenderer.DefaultLength = value;
                data.RenderRequired = true;
            }
        }

        private CrossSectionRenderer CrossSectionRenderer
        {
            get { return data.CustomRenderers.OfType<CrossSectionRenderer>().FirstOrDefault(); }
        }
    }
}