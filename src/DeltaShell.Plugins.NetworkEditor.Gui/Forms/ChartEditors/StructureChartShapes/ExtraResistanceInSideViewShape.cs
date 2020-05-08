using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class ExtraResistanceInSideViewShape : StructureSideViewShape<IExtraResistance>
    {
        private static readonly Bitmap ExtraResistanceSmallIcon = Resources.ExtraResistanceSmall;

        public ExtraResistanceInSideViewShape(IChart chart, double offsetInSideView, IExtraResistance structure)
            : base(chart, offsetInSideView, structure) {}

        protected override void CreateStyles() {}

        protected override IEnumerable<IShapeFeature> GetShapeFeatures()
        {
            yield return
                new SymbolShapeFeature(Chart, OffsetInSideView,
                                       ChartCoordinateService.ToWorldY(Chart, Chart.ChartBounds.Bottom),
                                       SymbolShapeFeatureHorizontalAlignment.Center,
                                       SymbolShapeFeatureVerticalAlignment.Bottom) {Image = ExtraResistanceSmallIcon};
        }
    }
}