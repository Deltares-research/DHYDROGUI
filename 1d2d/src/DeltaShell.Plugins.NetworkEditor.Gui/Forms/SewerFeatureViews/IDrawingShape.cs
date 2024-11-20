namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public interface IDrawingShape
    {
        object Source { get; set; }
        double TopLevel { get; set; }

        double BottomLevel { get; set; }

        double Width { get; set; }

        double Height { get; set; }

        double WidthPix { get; set; }

        double HeightPix { get; set; }

        #region Offset

        double TopOffset { get; set; }

        double TopOffsetPix { get; set; }

        double LeftOffset { get; set; }

        double LeftOffsetPix { get; set; }

        #endregion

        void SetPixelValues(double minX, double maxX, double minY, double maxY, double actualWidth, double actualHeight);

    }
}