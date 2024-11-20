using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapeEditors
{
    public class GatedWeirShapeEditor : CompositeShapeEditor
    {
        private RectangleShapeEditor WeirEditor { get; set; }
        private RectangleShapeEditor GateEditor { get; set; }

        public GatedWeirShapeEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService,
                                    ShapeEditMode shapeEditMode)
            : base(shapeFeature, chartCoordinateService, shapeEditMode)
        {
            WeirEditor = (RectangleShapeEditor) ShapeFeatureEditors[0];
        }

        /// <summary>
        /// Moves tracker for a gated weir. A gated weir consists of 2 shapefeatures. When the left(3), 
        /// right(1) or center(4) Trackers are moved update gate and weir
        /// trackerindices:
        ///           2
        /// 3         4         1
        ///           0
        /// </summary>
        /// <param name="trackerFeature"></param>
        /// <param name="worldPosition"></param>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <returns></returns>
        public override bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            int index = -1;

            foreach (IShapeFeatureEditor shapeFeatureEditor in ShapeFeatureEditors)
            {
                index = GetTrackerIndex(shapeFeatureEditor, trackerFeature);
                if (-1 != index)
                {
                    break;
                }
            }
            if (-1 == index)
            {
                return false;
            }
            if ((index == 0) || (index == 2))
            {
                base.MoveTracker(trackerFeature, worldPosition, deltaX, deltaY);
            }
            else
            {
                WeirEditor.MoveTracker(GetTracker(WeirEditor, index), worldPosition, deltaX, deltaY);
                GateEditor.MoveTracker(GetTracker(GateEditor, index), worldPosition, deltaX, deltaY);
            }
            return true;
        }
    }
}