using System.Drawing;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    internal static class NetworkSideViewStyles
    {
        internal static readonly Bitmap LateralSourceSmallIcon = Properties.Resources.LateralSourceSmall;
        internal static readonly Bitmap RetentionIcon = Properties.Resources.Retention;
        internal static readonly Bitmap ObservationIcon = Properties.Resources.Observation;

        internal static readonly VectorStyle NormalCrossSectionStyle = new VectorStyle
        {
            Fill = new SolidBrush(Color.FromArgb(100, Color.LightBlue)),
            Line = new Pen(Color.Black)
        };
        internal static readonly VectorStyle SelectedCrossSectionStyle = new VectorStyle
        {
            Fill = new SolidBrush(Color.LightBlue),
            Line = new Pen(Color.Black)
        };
        internal static readonly VectorStyle SelectStyle = new VectorStyle
        {
            Fill = new SolidBrush(Color.FromArgb(150, Color.Gray)),
            Line = new Pen(Color.Black)
        };
        internal static readonly VectorStyle DefaultStyle = new VectorStyle
        {
            Fill = Brushes.Transparent,
            Line = new Pen(Color.Black)
        };

        internal static readonly Color PipeColor = Color.Beige;

        internal static readonly Color WaterLevelColor = Color.RoyalBlue;

        internal static readonly Color MaxWaterLevelColor = Color.DarkBlue;
    }
}