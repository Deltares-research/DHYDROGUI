using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class ExtraResistanceInStructureViewShape : SymbolShapeFeature
    {
        private readonly IExtraResistance extraResistance;

        public ExtraResistanceInStructureViewShape(IChart chart, IExtraResistance extraResistance)
            : base(chart, 0, 0, SymbolShapeFeatureHorizontalAlignment.Left, SymbolShapeFeatureVerticalAlignment.Bottom)
        {
            this.extraResistance = extraResistance;
            Image = Properties.Resources.ExtraResistanceSmall;
        }

        /// <summary>
        /// Custom paint method since x of level lines is dependend of zoom-level
        /// </summary>
        /// <param name="vectorStyle"></param>
        public override void Paint(VectorStyle vectorStyle)
        {
            X = extraResistance.OffsetY;
            Y = ChartCoordinateService.ToWorldY(base.Chart, base.Chart.ChartBounds.Bottom);
            base.Paint(vectorStyle);
        }
    }
}