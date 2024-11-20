using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors
{
    public class CompositeShapeEditor : ShapeFeatureEditor
    {
        protected readonly List<IShapeFeatureEditor> ShapeFeatureEditors = new List<IShapeFeatureEditor>();

        public CompositeShapeEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode) : base(shapeFeature, chartCoordinateService, shapeEditMode)
        {
            var compositeShapeFeature = (CompositeShapeFeature) shapeFeature;

            foreach (var simpleShapeFeature in compositeShapeFeature.ShapeFeatures)
            {
                var accessor = simpleShapeFeature.CreateShapeFeatureEditor(shapeEditMode);
                if (accessor != null)
                {
                    ShapeFeatureEditors.Add(accessor);
                }
            }
        }

        public override bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            bool result = false;
            ShapeFeatureEditors.ForEach(e => result |= e.MoveTracker(trackerFeature, worldPosition, deltaX, deltaY));
            return result;
        }

        public override Cursor GetCursor(IPoint trackerFeature)
        {
            Cursor cursor = Cursors.Default;

            foreach (var shapeFeatureEditor in ShapeFeatureEditors)
            {
                cursor = shapeFeatureEditor.GetCursor(trackerFeature);
                if (cursor != Cursors.Default)
                {
                    return cursor;
                }
            }
            return cursor;
        }

        public override void Paint(IChart chart, ChartGraphics g)
        {
            ShapeFeatureEditors.ForEach(editor => editor.Paint(chart, g));
        }

        public override IPoint GetTrackerAt(double x, double y, double xMarge, double yMarge)
        {
            IPoint point = null;

            foreach (var shapeFeatureEditor in ShapeFeatureEditors)
            {
                point = shapeFeatureEditor.GetTrackerAt(x, y, xMarge, yMarge);
                if (point != null)
                {
                    return point;
                }
            }
            return point;
        }
    }
}