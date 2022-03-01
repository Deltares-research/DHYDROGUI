using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    internal sealed class ManHoleSideViewShape : FixedRectangleShapeFeature
    {
        private readonly double offsetInSideView;
        private readonly IManhole manhole;
        
        public ManHoleSideViewShape(IChart chart, double offsetInSideView, IManhole manhole) : base(chart)
        {
            this.offsetInSideView = offsetInSideView;
            this.manhole = manhole;
            WidthIsWorld = true;
            HeightIsWorld = true;
        }

        public override double X
        {
            get => offsetInSideView;
            set
            {
                // specified in constructor can not be changed
            }
        }

        /// <summary>
        /// The y coordinate of the shape. 
        /// if VerticalShapeAlignment is VerticalShapeAlignment.Top Y is the top coordinate of the bounding box
        /// if VerticalShapeAlignment is VerticalShapeAlignment.Center Y is the vertical center coordinate of the bounding box
        /// </summary>
        public override double Y
        {
            get => manhole.Compartments.Max(c => c.SurfaceLevel);
            set
            {
                // derived, can not be changed
            }
        }

        /// <summary>
        /// Height of the shape either in world coordinates (HeightIsWorld == true) of device coordinates (HeightIsWorld == false) 
        /// </summary>
        public override double Height
        {
            get => Y - manhole.Compartments.Min(c => c.BottomLevel);
            set
            {
                // derived, can not be changed
            }
        }

        public override double Width
        {
            get => GetDrawingWidth();
            set
            {
                // derived, can not be changed
            }
        }

        private double GetDrawingWidth()
        {
            const double relativeWidthToHorizontalAxis = 0.003;

            double horizontalAxisLength = Chart.BottomAxis.Maximum - Chart.BottomAxis.Minimum;
            double minimumDrawingWidth = horizontalAxisLength * relativeWidthToHorizontalAxis;
            double manholeWidth = manhole.Compartments.Sum(c => c.ManholeWidth);

            return manholeWidth > minimumDrawingWidth ? manholeWidth : minimumDrawingWidth;
        }
    }
}