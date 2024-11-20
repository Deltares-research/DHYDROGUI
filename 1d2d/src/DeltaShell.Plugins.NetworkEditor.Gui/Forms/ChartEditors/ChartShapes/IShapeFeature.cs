using System.Drawing;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    public interface IShapeFeature
    {
        IGeometry Geometry { get; set; }

        /// <summary>
        /// Checks whether x, y in world coordinates are inside the shape
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool Contains(double x, double y);

        /// <summary>
        /// Checks whether x, y in device coordinates are inside the shape
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool Contains(int x, int y);

        DelftTools.Controls.Swf.Charting.IChart Chart { get; set; }

        void Paint(VectorStyle style);

        bool Selected { get; set; }
        
        bool Active { get; set; }
        
        void Invalidate();

        IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode);

        Rectangle GetBounds();

        VectorStyle NormalStyle { get; set; }

        VectorStyle SelectedStyle { get; set; }

        VectorStyle DisabledStyle { get; set; }

        object Tag { get; set; }
    }
}