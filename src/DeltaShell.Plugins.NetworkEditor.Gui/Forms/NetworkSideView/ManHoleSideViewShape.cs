using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    internal class ManHoleSideViewShape : FixedRectangleShapeFeature
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
            get { return offsetInSideView; }
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
            get
            {
                return manhole.Compartments.Count != 0 
                    ? manhole.Compartments.Max(c => c.SurfaceLevel) 
                    : 0;
            }
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
            get
            {
                return manhole.Compartments.Count != 0 ? 
                    Y - manhole.Compartments.Min(c => c.BottomLevel) 
                    : 0;
            }
            set
            {
                // derived, can not be changed
            }
        }

        public override double Width
        {
            get
            {
                return manhole.Compartments.Count > 0
                    ? manhole.Compartments.Sum(c => c.ManholeWidth)
                    : 0;
            }
            set
            {
                // derived, can not be changed
            }
        }
    }
}