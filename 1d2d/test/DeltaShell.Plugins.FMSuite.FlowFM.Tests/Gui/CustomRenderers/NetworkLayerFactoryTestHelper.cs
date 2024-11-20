using System.Drawing;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.CustomRenderers
{
    static internal class NetworkLayerFactoryTestHelper
    {
        public static bool CompareImages(Bitmap inputImage1, Bitmap inputImage2)
        {
            for (int x = 0; x < inputImage1.Width; x++)
            {
                for (int y = 0; y < inputImage1.Height; y++)
                {
                    if (inputImage1.GetPixel(x, y) != inputImage2.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}