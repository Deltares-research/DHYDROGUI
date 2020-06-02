using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;
using SharpMap.Converters.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors
{
    public class SymbolShapeEditor : ShapeFeatureEditor, IShapeFeatureEditor
    {
        public SymbolShapeEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode)
            : base(shapeFeature, chartCoordinateService, shapeEditMode)
        {
            if (CanMove)
            {
                CenterTracker = GeometryFactory.CreatePoint(0, 0);
            }
        }

        public bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            var symbolShapeFeature = (SymbolShapeFeature) ShapeFeature;
            if (CanMove)
            {
                symbolShapeFeature.X += deltaX;
                symbolShapeFeature.Y += deltaY;
            }

            return true;
        }

        public override Cursor GetCursor(IPoint trackerFeature)
        {
            return CanMove ? Cursors.SizeAll : base.GetCursor(trackerFeature);
        }
    }
}