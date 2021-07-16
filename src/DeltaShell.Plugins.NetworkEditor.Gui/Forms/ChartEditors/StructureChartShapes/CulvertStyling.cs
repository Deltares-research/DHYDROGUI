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
        static CulvertStyling()
        {
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

        private const int alpha = 40;

        public static Color OutletColor = Color.CornflowerBlue;
        public static Color InletColor = Color.MidnightBlue;

        //styles are shared between shapes in structureview and sideview..
        public static VectorStyle SelectedStyle { get; private set; }
        public static VectorStyle NormalStyle { get; private set; }
        public static VectorStyle NormalInletStyle { get; private set; }
        public static VectorStyle SelectedInletStyle { get; private set; }

        public static VectorStyle NormalOutletStyle { get; private set; }
        public static VectorStyle SelectedOutletStyle { get; set; }
    }
}