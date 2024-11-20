using System;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapeEditors;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    /// <summary>
    /// A shapefeature to display the crest of weirs in a side view
    /// It has a special property CrestOffset to amnipulate the size of the crest. This is typically the only
    /// property a user is allowed to manupulate.
    /// 
    ///    /-\
    ///   /   \
    ///  /     \
    /// /       \
    /// |   x   |  ^
    /// |       |  |  CrestOffset
    /// |       |  v
    /// ---------
    /// 
    /// </summary>
    class CrestShapeFeature : FixedRectangleShapeFeature
    {
        public double CrestOffset { get; set; }

        public CrestShapeFeature(IChart chart, double x, double top, double pixelWidth, double bottom, double crestOffset)
            : base(chart, x, top, pixelWidth, Math.Max(0, top - bottom), false, true)
        {
            HorizontalShapeAlignment = HorizontalShapeAlignment.Center;
            CrestOffset = crestOffset;
            Bottom = bottom;
        }

        public double Bottom { get; private set; }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return new CrestShapeEditor(this, new ChartCoordinateService(Chart), shapeEditMode);
        }
    }
}