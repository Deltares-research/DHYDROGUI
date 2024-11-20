using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class CrossSectionInSideViewShape : FixedRectangleShapeFeature
    {
        public CrossSectionInSideViewShape(IChart chart, double offsetInSideView, int width, ICrossSectionDefinition crossSectionDefinition)
            : base(chart)
        {
            CrossSectionDefinition = crossSectionDefinition;
            OffsetInSideView = offsetInSideView;
            WidthIsWorld = false;
            HeightIsWorld = true;
            Width = width;
        }

        protected ICrossSectionDefinition CrossSectionDefinition
        {
            get;
            private set;
        }

        protected double OffsetInSideView
        {
            get;
            private set;
        }

        public override double X
        {
            get
            {
                return OffsetInSideView;
            }
            set { }
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
                return CrossSectionDefinition.HighestPoint;
            } 
            set { }
        }

        /// <summary>
        /// Height of the shape either in world coordinates (HeightIsWorld == true) of device coordinates (HeightIsWorld == false) 
        /// </summary>
        public override double Height
        {
            get
            {
                return CrossSectionDefinition.HighestPoint - CrossSectionDefinition.LowestPoint;
            } 
            set { }
        }
    }
}
