using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class ExtraResistanceInSideViewShape : StructureSideViewShape<IExtraResistance>
    {
        private static readonly Bitmap ExtraResistanceSmallIcon = Properties.Resources.ExtraResistanceSmall;
        private readonly double iconLocationY;

        public ExtraResistanceInSideViewShape(IChart chart, 
                                              double offsetInSideView, 
                                              double iconLocationY,
                                              IExtraResistance structure) 
            : base(chart, offsetInSideView, structure)
        {
            this.iconLocationY = iconLocationY;
        }

        protected override void CreateStyles()
        {
        }

        protected override IEnumerable<IShapeFeature> GetShapeFeatures()
        {
            yield return
                new SymbolShapeFeature(Chart, 
                                       OffsetInSideView,
                                       iconLocationY,
                                       SymbolShapeFeatureHorizontalAlignment.Center,
                                       SymbolShapeFeatureVerticalAlignment.Center)
                    {
                        Image = ExtraResistanceSmallIcon
                    };
        }
    }
}