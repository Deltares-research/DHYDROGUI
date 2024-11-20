using DelftTools.Controls.Swf.Charting;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    class BroadCrestShapeFeature : CrestShapeFeature
    {
        /// |---x---|  ^
        /// |       |  |  CrestOffset
        /// |       |  v
        /// ---------

        public BroadCrestShapeFeature(IChart chart, double x, double top, double pixelWidth, double bottom) 
            : base(chart, x, top, pixelWidth, bottom, 0.0)
        {
        }
    }
}