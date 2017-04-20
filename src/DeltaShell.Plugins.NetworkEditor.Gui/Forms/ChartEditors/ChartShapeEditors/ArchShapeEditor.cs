using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using SharpMap.Converters.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors
{
    public class ArchShapeEditor : RectangleShapeEditor
    {
        public ArchShapeEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode)
            : base(shapeFeature, chartCoordinateService, shapeEditMode)
        {
            points.Add(GeometryFactory.CreatePoint(0, 0));
            UpdateArchTrackers();
        }

        public override Cursor GetCursor(IPoint trackerFeature)
        {
            return trackerFeature == points[4] ? Cursors.SizeNS : base.GetCursor(trackerFeature);
        }

        private void UpdateArchTrackers()
        {
            RoundCrestShapeFeature roundCrestShapeFeature = (RoundCrestShapeFeature)ShapeFeature;
            // arch offset tracker
            points[4].X = roundCrestShapeFeature.X;
            points[4].Y = (roundCrestShapeFeature.Y - roundCrestShapeFeature.Height) + roundCrestShapeFeature.CrestOffset;
            points[4].GeometryChangedAction();
        }

        protected override void UpdateTrackers()
        {
            base.UpdateTrackers();
            UpdateArchTrackers();
        }
        public override bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            RoundCrestShapeFeature roundCrestShapeFeature = (RoundCrestShapeFeature)ShapeFeature;
            if (trackerFeature == points[4])
            {
                // arch offset tracker
                roundCrestShapeFeature.CrestOffset += deltaY;
                GeometryHelper.MoveCoordinate(points[4], 0, 0, deltaY);
                points[4].GeometryChangedAction();
                UpdateTrackers();
                return true;
            }
            return base.MoveTracker(trackerFeature, worldPosition, deltaX, deltaY);
        }
    }
}