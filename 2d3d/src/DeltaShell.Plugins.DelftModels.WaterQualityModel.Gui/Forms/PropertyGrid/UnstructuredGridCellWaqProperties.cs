using System.ComponentModel;
using DeltaShell.Plugins.SharpMapGis.Gui.ObjectProperties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    public class UnstructuredGridCellWaqProperties : UnstructuredGridFeatureProperties
    {
        [Category("General")]
        [DisplayName("Segment index")]
        [Description("Segment index for the grid")]
        public int SegmentIndex => data.Index + 1;
    }
}