using System.Drawing;
using DelftTools.Controls.Swf.Charting;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    public enum SymbolShapeFeatureHorizontalAlignment
    {
        Left,
        Center,
        Right
    }
    public enum SymbolShapeFeatureVerticalAlignment
    {
        Top,
        Center,
        Bottom
    }
    public class SymbolShapeFeature : ShapeFeatureBase
    {
        public SymbolShapeFeature(IChart chart, double x, double y, 
                                  SymbolShapeFeatureHorizontalAlignment symbolShapeFeatureHorizontalAlignment,
                                  SymbolShapeFeatureVerticalAlignment symbolShapeFeatureVerticalAlignment
            )
            : base(chart)
        {
            X = x;
            Y = y;
            SymbolShapeFeatureHorizontalAlignment = symbolShapeFeatureHorizontalAlignment;
            SymbolShapeFeatureVerticalAlignment = symbolShapeFeatureVerticalAlignment;
        }

        public Image Image{ get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public SymbolShapeFeatureHorizontalAlignment SymbolShapeFeatureHorizontalAlignment { get; set; }
        public SymbolShapeFeatureVerticalAlignment SymbolShapeFeatureVerticalAlignment { get; set; }

        public double Left
        {
            get
            {
                switch (SymbolShapeFeatureHorizontalAlignment)
                {
                    case SymbolShapeFeatureHorizontalAlignment.Left:
                        return X;
                    case SymbolShapeFeatureHorizontalAlignment.Right:
                        return X + ChartCoordinateService.ToWorldWidth(Chart, Image.Width);
                    default:
                        return X - ChartCoordinateService.ToWorldWidth(Chart, Image.Width)/2;
                }
            }
        }

        public override Rectangle GetBounds()
        {
            int x = ChartCoordinateService.ToDeviceX(Chart, X);
            int y = ChartCoordinateService.ToDeviceY(Chart, Y);
            switch (SymbolShapeFeatureHorizontalAlignment)
            {
                case SymbolShapeFeatureHorizontalAlignment.Left:
                    break;
                case SymbolShapeFeatureHorizontalAlignment.Center:
                    x -= (Image.Width / 2);
                    break;
                case SymbolShapeFeatureHorizontalAlignment.Right:
                    x += Image.Width;
                    break;
            }
            switch (SymbolShapeFeatureVerticalAlignment)
            {
                case SymbolShapeFeatureVerticalAlignment.Top:
                    break;
                case SymbolShapeFeatureVerticalAlignment.Center:
                    y -= (Image.Height/2);
                    break;
                case SymbolShapeFeatureVerticalAlignment.Bottom:
                    y -= Image.Height;
                    break;
            }
            Rectangle rectangle = new Rectangle
                                      {
                                          X = x,
                                          Y = y,
                                          Width = Image.Width,
                                          Height = Image.Height
                                      };
            return rectangle;
        }

        protected override void Paint(IChartDrawingContext chartDrawingContext)
        {
            var g = (ChartGraphics)chartDrawingContext.Graphics;
            if (!Active)
            {
                g.Draw(GetBounds(), GetGraysImage(Image), false);
            }
            else if (Selected)
            {
                g.Draw(GetBounds(), Image, false);
                g.BackColor = Color.FromArgb(100, Color.Magenta);
                g.Rectangle(GetBounds());
            }
            else
            {
                g.Draw(GetBounds(), Image, false);
            }
        }

        private static Image GetGraysImage(Image original)
        {
            
            var originalBitmap = new Bitmap(original);
            //make an empty bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            
            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    //get the pixel from the original image
                    Color originalColor = originalBitmap.GetPixel(i, j);

                    //create the grayscale version of the pixel
                    int grayScale = (int)((originalColor.R * .3) + (originalColor.G * .59)
                        + (originalColor.B * .11));

                    //create the color object
                    byte alpha;
                    if (originalColor.A!= 0)
                    {
                        alpha = 80;
                    }
                    else
                    {
                        alpha = originalColor.A;
                    }

                    Color newColor = Color.FromArgb(alpha, grayScale, grayScale,grayScale);
                    
                    //set the new image's pixel to the grayscale version
                    newBitmap.SetPixel(i, j, newColor);
                }
            }

            return newBitmap;
        }
    }
}