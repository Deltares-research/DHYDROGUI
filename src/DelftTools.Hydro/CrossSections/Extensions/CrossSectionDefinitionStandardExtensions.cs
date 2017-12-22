using System;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DelftTools.Hydro.CrossSections.Extensions
{
    public static class CrossSectionDefinitionStandardExtensions
    {
        public static double GetProfileDiameter(this CrossSectionDefinitionStandard csDefinition)
        {
            var roundShape = csDefinition?.Shape as CrossSectionStandardShapeRound;
            return roundShape?.Diameter ?? double.NaN;
        }

        public static double GetProfileWidth(this CrossSectionDefinitionStandard csDefinition)
        {
            var shape = csDefinition?.Shape;
            var widthBasedShape = shape as CrossSectionStandardShapeWidthHeightBase;
            if (widthBasedShape != null) return Math.Round(widthBasedShape.Width, 2, MidpointRounding.AwayFromZero);

            var archShape = shape as CrossSectionStandardShapeArch;
            return archShape != null ? Math.Round(archShape.Width, 2, MidpointRounding.AwayFromZero) : double.NaN;
        }

        public static double GetProfileHeight(this CrossSectionDefinitionStandard csDefinition)
        {
            var shape = csDefinition?.Shape;
            var widthBasedShape = shape as CrossSectionStandardShapeWidthHeightBase;
            if (widthBasedShape != null) return Math.Round(widthBasedShape.Height, 2, MidpointRounding.AwayFromZero);

            var archShape = shape as CrossSectionStandardShapeArch;
            return archShape != null ? Math.Round(archShape.Height, 2, MidpointRounding.AwayFromZero) : double.NaN;
        }

        public static double GetProfileArchHeight(this CrossSectionDefinitionStandard csDefinition)
        {
            var archShape = csDefinition?.Shape as CrossSectionStandardShapeArch;
            return archShape != null ? Math.Round(archShape.ArcHeight, 2, MidpointRounding.AwayFromZero) : double.NaN;
        }

        public static double GetProfileSlope(this CrossSectionDefinitionStandard csDefinition)
        {
            var trapezoidShape = csDefinition?.Shape as CrossSectionStandardShapeTrapezium;
            return trapezoidShape != null ? Math.Round(trapezoidShape.Slope, 2, MidpointRounding.AwayFromZero) : double.NaN;
        }

        public static double GetProfileBottomWidthB(this CrossSectionDefinitionStandard csDefinition)
        {
            var trapezoidShape = csDefinition?.Shape as CrossSectionStandardShapeTrapezium;
            return trapezoidShape != null ? Math.Round(trapezoidShape.BottomWidthB, 2, MidpointRounding.AwayFromZero) : double.NaN;
        }

        public static double GetProfileMaximumFlowWidth(this CrossSectionDefinitionStandard csDefinition)
        {
            var trapezoidShape = csDefinition?.Shape as CrossSectionStandardShapeTrapezium;
            return trapezoidShape != null ? Math.Round(trapezoidShape.MaximumFlowWidth, 2, MidpointRounding.AwayFromZero) : double.NaN;
        }
    }
}
