using System;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Converter for converting a boundary mdw category to a <see cref="BoundaryMdwBlock" />.
    /// </summary>
    public static class BoundaryCategoryConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="boundaryCategory" />
        /// to a <see cref="BoundaryMdwBlock" />.
        /// </summary>
        /// <param name="boundaryCategory"> The boundary delft ini category. </param>
        /// <returns>
        /// The created boundary mdw block.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaryCategory" /> is <c> null </c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="boundaryCategory" /> is not an mdw boundary category.
        /// </exception>
        public static BoundaryMdwBlock Convert(DelftIniCategory boundaryCategory)
        {
            Ensure.NotNull(boundaryCategory, nameof(boundaryCategory));

            if (boundaryCategory.Name != KnownWaveCategories.BoundaryCategory)
            {
                throw new ArgumentException("Category is not an mdw boundary category.",
                                            nameof(boundaryCategory));
            }

            return new BoundaryMdwBlock(boundaryCategory.GetPropertyValue(KnownWaveProperties.Name))
            {
                Definition = boundaryCategory.GetPropertyValue(KnownWaveProperties.Definition),
                XStartCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.StartCoordinateX),
                YStartCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.StartCoordinateY),
                XEndCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.EndCoordinateX),
                YEndCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.EndCoordinateY),
                SpectrumType = boundaryCategory.GetEnumValue<SpectrumType>(KnownWaveProperties.SpectrumSpec),
                ShapeType = boundaryCategory.GetEnumValue<ShapeType>(KnownWaveProperties.ShapeType),
                PeriodType = boundaryCategory.GetEnumValue<PeriodType>(KnownWaveProperties.PeriodType),
                SpreadingType = boundaryCategory.GetEnumValue<SpreadingType>(KnownWaveProperties.DirectionalSpreadingType),
                PeakEnhancementFactor = boundaryCategory.GetDoubleValue(KnownWaveProperties.PeakEnhancementFactor),
                Spreading = boundaryCategory.GetDoubleValue(KnownWaveProperties.GaussianSpreading),
                Distances = boundaryCategory.GetDoubleValues(KnownWaveProperties.CondSpecAtDist),
                WaveHeights = boundaryCategory.GetDoubleValues(KnownWaveProperties.WaveHeight),
                Periods = boundaryCategory.GetDoubleValues(KnownWaveProperties.Period),
                Directions = boundaryCategory.GetDoubleValues(KnownWaveProperties.Direction),
                DirectionalSpreadings = boundaryCategory.GetDoubleValues(KnownWaveProperties.DirectionalSpreadingValue)
            };
        }

        private static double ToDouble(this string value)
        {
            return double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        private static T GetEnumValue<T>(this DelftIniCategory category, string propertyName)
        {
            return EnumUtils.GetEnumValueByDescription<T>(category.GetPropertyValue(propertyName).ToLower());
        }

        private static double GetDoubleValue(this DelftIniCategory category, string propertyName)
        {
            return category.GetPropertyValue(propertyName, double.NaN.ToString(CultureInfo.InvariantCulture)).ToDouble();
        }

        private static double[] GetDoubleValues(this DelftIniCategory category, string propertyName)
        {
            return category.GetPropertyValues(propertyName).Select(ToDouble).ToArray();
        }
    }
}