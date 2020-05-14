using System.Drawing;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    //Classes containing color information for structures. Merge with StructureShapeStyle provider

    /// <summary>
    /// Colors used in culvertView and SideView,StructureView shapes
    /// </summary>
    public static class CulvertStyling
    {
        private static readonly Color SiphonOffLevelColor = Color.Red;
        private static readonly Color SiphonOnLevelColor = Color.Black;

        private static readonly Color OutletColor = Color.CornflowerBlue;
        private static readonly Color InletColor = Color.MidnightBlue;

        private const int alpha = 40;

        static CulvertStyling()
        {
            NormalSiphonOffLevelStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.FromArgb(alpha, SiphonOffLevelColor)),
                Line = new Pen(Color.FromArgb(alpha, SiphonOffLevelColor))
            };

            SelectedSiphonOffLevelStyle = new VectorStyle
            {
                Fill = new SolidBrush(SiphonOffLevelColor),
                Line = new Pen(SiphonOffLevelColor)
            };

            NormalSiphonOnLevelStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.FromArgb(alpha, SiphonOnLevelColor)),
                Line = new Pen(Color.FromArgb(alpha, SiphonOnLevelColor))
            };

            SelectedSiphonOnLevelStyle = new VectorStyle
            {
                Fill = new SolidBrush(SiphonOnLevelColor),
                Line = new Pen(SiphonOnLevelColor)
            };

            SelectedStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.LightBlue),
                Line = new Pen(Color.Black)
            };
            NormalStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.FromArgb(alpha, Color.LightBlue)),
                Line = new Pen(Color.FromArgb(alpha, Color.Black))
            };

            NormalInletStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.FromArgb(alpha, InletColor)),
                Line = new Pen(Color.FromArgb(alpha, InletColor))
            };
            SelectedInletStyle = new VectorStyle
            {
                Fill = new SolidBrush(InletColor),
                Line = new Pen(InletColor)
            };

            NormalOutletStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.FromArgb(alpha, OutletColor)),
                Line = new Pen(Color.FromArgb(alpha, OutletColor))
            };
            SelectedOutletStyle = new VectorStyle
            {
                Fill = new SolidBrush(OutletColor),
                Line = new Pen(OutletColor)
            };
        }

        //styles are shared between shapes in structureview and sideview..
        public static VectorStyle SelectedStyle { get; private set; }
        public static VectorStyle NormalStyle { get; private set; }

        public static VectorStyle NormalSiphonOffLevelStyle { get; private set; }
        public static VectorStyle NormalSiphonOnLevelStyle { get; private set; }
        public static VectorStyle SelectedSiphonOffLevelStyle { get; private set; }
        public static VectorStyle SelectedSiphonOnLevelStyle { get; private set; }

        public static VectorStyle NormalInletStyle { get; private set; }
        public static VectorStyle SelectedInletStyle { get; private set; }

        public static VectorStyle NormalOutletStyle { get; private set; }
        public static VectorStyle SelectedOutletStyle { get; set; }
    }
}