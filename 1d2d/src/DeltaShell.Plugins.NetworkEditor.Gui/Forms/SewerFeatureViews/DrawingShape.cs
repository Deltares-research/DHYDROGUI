using DelftTools.Utils.Aop;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public abstract class DrawingShape : IDrawingShape
    {
        public virtual object Source { get; set; }
        public virtual double TopLevel { get; set; }
        public virtual double BottomLevel { get; set; }
        public virtual double Width { get; set; }
        public virtual double Height { get; set; }
        public virtual double WidthPix { get; set; }
        public virtual double HeightPix { get; set; }
        public virtual double TopOffset { get; set; }
        public virtual double TopOffsetPix { get; set; }
        public virtual double LeftOffset { get; set; }
        public virtual double LeftOffsetPix { get; set; }
        public void SetPixelValues(double minX, double maxX, double minY, double maxY, double actualWidth, double actualHeight)
        {
            LeftOffsetPix = CoordinateScalingHelper.ScaleX(LeftOffset, minX, maxX, actualWidth);
            TopOffsetPix = CoordinateScalingHelper.ScaleY(TopLevel, minY, maxY, actualHeight);
            WidthPix = CoordinateScalingHelper.ScaleWidth(Width, minX, maxX, actualWidth);
            HeightPix = CoordinateScalingHelper.ScaleHeight(Height, minY, maxY, actualHeight);
        }
    }
}