using System;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DelftTools.Hydro.CrossSections.Extensions
{
    public static class CrossSectionDefinitionStandardExtensions
    {
        public static double GetProfileDiameter(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeRound>(shape => shape.Diameter);
        }

        public static double GetProfileWidth(this CrossSectionDefinitionStandard csDefinition)
        {
            var value = csDefinition.GetPropertyValue<CrossSectionStandardShapeWidthHeightBase>(shape => shape.Width);
            return double.IsNaN(value) ? csDefinition.GetPropertyValue<CrossSectionStandardShapeArch>(shape => shape.Width) : value;
        }

        public static double GetProfileHeight(this CrossSectionDefinitionStandard csDefinition)
        {
            var value = csDefinition.GetPropertyValue<CrossSectionStandardShapeWidthHeightBase>(shape => shape.Height);
            return double.IsNaN(value) ? csDefinition.GetPropertyValue<CrossSectionStandardShapeArch>(shape => shape.Height) : value;
        }

        public static double GetProfileArchHeight(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeArch>(shape => shape.ArcHeight);
        }

        public static double GetProfileSlope(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeTrapezium>(shape => shape.Slope);
        }

        public static double GetProfileBottomWidthB(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeTrapezium>(shape => shape.BottomWidthB);
        }

        public static double GetProfileMaximumFlowWidth(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeTrapezium>(shape => shape.MaximumFlowWidth);
        }

        private static double GetPropertyValue<T>(this CrossSectionDefinitionStandard csDefinition, Func<T, double> function) 
            where T : CrossSectionStandardShapeBase
        {
            var shape = csDefinition?.Shape as T;
            return shape != null
                ? Math.Round(function(shape), 2, MidpointRounding.AwayFromZero)
                : double.NaN;
        }
    }
}
