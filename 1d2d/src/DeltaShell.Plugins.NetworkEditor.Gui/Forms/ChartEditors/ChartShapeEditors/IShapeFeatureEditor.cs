using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors
{
    public interface IShapeFeatureEditor
    {
        IShapeFeature ShapeFeature { get; set; }

        IPoint CurrentTracker { get; set; }

        IEnumerable<IPoint> GetTrackers();
        
        bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY);
        
        IPoint GetTrackerAt(double x, double y, double width, double height);
        
        Cursor GetCursor(IPoint trackerFeature);
        
        void Paint(IChart chart, ChartGraphics g);
        
        SnapResult Snap(Coordinate worldPosition, double width, double height);
        
        void InsertCoordinate(Coordinate worldPosition, double width, double height);

        void DeleteTracker(IPoint trackerFeature);
        
        bool CanDeleteTracker(IPoint trackerFeature);

        void Start();
        
        void Stop();
    }
}