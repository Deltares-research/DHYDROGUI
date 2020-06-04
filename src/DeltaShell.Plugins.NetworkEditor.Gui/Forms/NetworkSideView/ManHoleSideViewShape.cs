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

        public ManHoleSideViewShape(IChart chart, double offsetInSideView, int width, IManhole manhole) : base(chart)
        {
            this.offsetInSideView = offsetInSideView;
            this.manhole = manhole;
            WidthIsWorld = false;
            HeightIsWorld = true; 
            Width = width;
        }

        public override double X
        {
            get { return offsetInSideView; }
            set { }
        }

        /// <summary>
        /// The y coordinate of the shape. 
        /// if VerticalShapeAlignment is VerticalShapeAlignment.Top Y is the top coordinate of the bounding box
        /// if VerticalShapeAlignment is VerticalShapeAlignment.Center Y is the vertical center coordinate of the bounding box
        /// </summary>
        public override double Y
        {
            get { return manhole.Compartments.Max(c => c.SurfaceLevel); }
            set { }
        }

        ///// <summary>
        ///// Width of the shape either in world coordinates (WidthIsWorld == true) of device coordinates (WidthIsWorld == false) 
        ///// </summary>
        //public override double Width { get; set; }

        /// <summary>
        /// Height of the shape either in world coordinates (HeightIsWorld == true) of device coordinates (HeightIsWorld == false) 
        /// </summary>
        public override double Height
        {
            get
            {
                return Y - manhole.Compartments.Min(c => c.BottomLevel);
            }
            set { }
        }
    }
}