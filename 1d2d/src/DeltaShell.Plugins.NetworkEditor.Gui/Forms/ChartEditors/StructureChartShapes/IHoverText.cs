using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public enum HoverType
    {
        Always,
        Selected
    }

    public enum HoverPosition
    {
        Top,
        Right,
        Bottom,
        Left
    }

    public enum ArrowHeadPosition
    {
        None,
        Top,
        Right,
        Down,
        Left,
        TopDown,
        LeftRight
    }

    public interface IHoverFeature
    {
        Color ForeColor { get; set; }
        IShapeFeature ShapeFeature { get; set; }
        void Render(List<Rectangle> usedSpace, IChart chart, Graphics graphics);
        HoverType HoverType { get; set; }
    }

    public interface IHoverText : IHoverFeature
    {
        string Line1 { get; set; }
        string Line2 { get; set; }
        HoverPosition HoverPosition { get; set; }
        ArrowHeadPosition ArrowHeadPosition { get; set; }
        bool ShowLine { get; set; }
    }
}