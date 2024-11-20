using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DelftTools.Controls.Wpf.ValueConverters;

namespace DeltaShell.NGHS.Common.Gui
{
    public static class BitmapConversionExtensions
    {
        public static Bitmap BitmapFromSource(this BitmapSource bitmapSource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }

            return bitmap;
        }

        public static Bitmap BitmapFromBrush(this DrawingBrush brush, int width, int height)
        {
            var converter = new BrushToBitmapConverter {Height = height, Width = width };
            var bitmapSource = (BitmapSource) converter.Convert(brush, typeof(BitmapSource), null, null);
            return  bitmapSource.BitmapFromSource();
        }
    }
}
