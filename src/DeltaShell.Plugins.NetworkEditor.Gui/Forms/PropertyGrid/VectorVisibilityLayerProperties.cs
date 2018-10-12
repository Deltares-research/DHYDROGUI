using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.GridProperties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [DisplayName("Vector visibility layer")]
    public class VisibilityVectorLayerProperties : VectorLayerProperties
    {
        [Description("Maximum visibility on scale")]
        [Category("Rendering visibility")]
        [PropertyOrder(1)]
        public double MaxVisible
        {
            get { return data.MaxVisible; }
            set { data.MaxVisible = value; }
        }

        [Description("Minimum visibility on scale")]
        [Category("Rendering visibility")]
        [PropertyOrder(2)]
        public double MinVisible
        {
            get { return data.MinVisible; }
            set { data.MinVisible = value; }
        }
    }
}