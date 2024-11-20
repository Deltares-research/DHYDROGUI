using System.Collections.Generic;
using System.Drawing;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public interface IHover
    {
        void Hover(List<Rectangle> usedSpace, VectorStyle style, Graphics graphics);
        void AddHover(IHoverFeature hoverText);
        void ClearHovers();
    }
}