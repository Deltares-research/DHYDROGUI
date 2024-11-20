using System;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DelftTools.Hydro.CrossSections.Extensions
{
    public static class CrossSectionDefinitionStandardExtensions
    {
        public static double GetProfileDiameter(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeCircle>(shape => shape.Diameter, 3);
        }

        public static double GetProfileWidth(this CrossSectionDefinitionStandard csDefinition)
        {
            var value = csDefinition.GetPropertyValue<CrossSectionStandardShapeWidthHeightBase>(shape => shape.Width, 3);
            return double.IsNaN(value) ? csDefinition.GetPropertyValue<CrossSectionStandardShapeArch>(shape => shape.Width, 3) : value;
        }

        public static double GetProfileHeight(this CrossSectionDefinitionStandard csDefinition)
        {
            var value = csDefinition.GetPropertyValue<CrossSectionStandardShapeWidthHeightBase>(shape => shape.Height, 3);
            return double.IsNaN(value) ? csDefinition.GetPropertyValue<CrossSectionStandardShapeArch>(shape => shape.Height, 3) : value;
        }

        public static double GetProfileArchHeight(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeArch>(shape => shape.ArcHeight, 2);
        }

        public static double GetProfileSlope(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeTrapezium>(shape => shape.Slope, 2);
        }

        public static double GetProfileBottomWidthB(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeTrapezium>(shape => shape.BottomWidthB, 2);
        }

        public static double GetProfileMaximumFlowWidth(this CrossSectionDefinitionStandard csDefinition)
        {
            return csDefinition.GetPropertyValue<CrossSectionStandardShapeTrapezium>(shape => shape.MaximumFlowWidth, 2);
        }

        private static double GetPropertyValue<T>(this CrossSectionDefinitionStandard csDefinition, Func<T, double> function, int numOfDigits) 
            where T : CrossSectionStandardShapeBase
        {
            var shape = csDefinition?.Shape as T;
            return shape != null
                ? Math.Round(function(shape), numOfDigits, MidpointRounding.AwayFromZero)
                : double.NaN;
        }
    }
}
