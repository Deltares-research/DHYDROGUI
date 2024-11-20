using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Tools;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public class ShapeLayerTool : IShapeLayerTool
    {
        public virtual bool IsActive { get; set; }
        public virtual bool IsBusy { get; set; }
        public ShapeModifyTool ShapeModifyTool { get; set; }

        public virtual void Paint()
        {
        }

        public virtual void MouseEvent(ChartMouseEvent kind, MouseEventArgs e, Cursor c)
        {
        }

        public virtual SnapResult Snap(IShapeFeatureEditor shapeFeatureEditor, Coordinate worldPosition)
        {
            const int tolerance = 4;
            double worldWidth = ChartCoordinateService.ToWorldWidth(ShapeModifyTool.Chart, tolerance);
            double worldHeight = ChartCoordinateService.ToWorldHeight(ShapeModifyTool.Chart, tolerance);
            return shapeFeatureEditor.Snap(worldPosition, worldWidth, worldHeight);
        }
    }
}