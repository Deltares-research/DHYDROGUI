using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapeEditors
{
    public class CrestShapeEditor : ShapeFeatureEditor
    {
        public CrestShapeEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService,
                                ShapeEditMode shapeEditMode) : base(shapeFeature, chartCoordinateService, shapeEditMode)
        {
            if ((CanResize) || (CanMove))
            {
                points.Add(new Point(0, 0));
            }
            UpdateTrackers();
        }

        protected void UpdateTrackers()
        {
            CrestShapeFeature crestShapeFeature = (CrestShapeFeature)ShapeFeature;
            // arch offset tracker
            if (points.Count > 0)
            {
                points[0].X = crestShapeFeature.X;
                points[0].Y = crestShapeFeature.Bottom;
                points[0].GeometryChangedAction();    
            }
            
        }

        public override Cursor GetCursor(IPoint trackerFeature)
        {
            return (trackerFeature != null) ? Cursors.SizeNS : Cursors.Default;
        }

        public override bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            CrestShapeFeature feature = (CrestShapeFeature)ShapeFeature;
            feature.CrestOffset += deltaY;
            feature.Y += deltaY;
            GeometryHelper.MoveCoordinate(points[0], 0, 0, deltaY);
            points[0].GeometryChangedAction();
            UpdateTrackers();
            return true;
        }
    }
}