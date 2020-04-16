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
    /// Converter for converting a boundary mdw category to a <see cref="BoundaryMdwBlock"/>.
    /// </summary>
    public static class BoundaryCategoryConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="boundaryCategory"/>
        /// to a <see cref="BoundaryMdwBlock"/>.
        /// </summary>
        /// <param name="boundaryCategory"> The boundary delft ini category. </param>
        /// <returns>
        /// The created boundary mdw block.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaryCategory"/> is <c> null </c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="boundaryCategory"/> is not an mdw boundary category.
        /// </exception>
        public static BoundaryMdwBlock Convert(DelftIniCategory boundaryCategory)
        {
            Ensure.NotNull(boundaryCategory, nameof(boundaryCategory));

            if (boundaryCategory.Name != KnownWaveCategories.BoundaryCategory)
            {
                throw new ArgumentException("Category is not an mdw boundary category.", nameof(boundaryCategory));
            }

            var block = new BoundaryMdwBlock(boundaryCategory.GetPropertyValue(KnownWaveProperties.Name))
            {
                DefinitionType = boundaryCategory.GetEnumValue<DefinitionImportType>(KnownWaveProperties.Definition),
                XStartCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.StartCoordinateX),
                YStartCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.StartCoordinateY),
                XEndCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.EndCoordinateX),
                YEndCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.EndCoordinateY),
                SpectrumType = boundaryCategory.GetEnumValue<SpectrumImportType>(KnownWaveProperties.SpectrumSpec),
                Distances = boundaryCategory.GetDoubleValues(KnownWaveProperties.CondSpecAtDist)
            };

            if (block.SpectrumType == SpectrumImportType.Parametrized)
            {
                ConvertParameterizedProperties(boundaryCategory, block);
            }
            else if (block.SpectrumType == SpectrumImportType.FromFile)
            {
                block.SpectrumFiles = boundaryCategory.GetStringValues(KnownWaveProperties.Spectrum);
            }

            return block;
        }

        private static void ConvertParameterizedProperties(DelftIniCategory boundaryCategory, BoundaryMdwBlock block)
        {
            block.ShapeType = boundaryCategory.GetEnumValue<ShapeImportType>(KnownWaveProperties.ShapeType);
            block.PeriodType = boundaryCategory.GetEnumValue<PeriodImportExportType>(KnownWaveProperties.PeriodType);
            block.SpreadingType = boundaryCategory.GetEnumValue<SpreadingImportType>(KnownWaveProperties.DirectionalSpreadingType);
            block.PeakEnhancementFactor = boundaryCategory.GetDoubleValue(KnownWaveProperties.PeakEnhancementFactor);
            block.Spreading = boundaryCategory.GetDoubleValue(KnownWaveProperties.GaussianSpreading);
            block.WaveHeights = boundaryCategory.GetDoubleValues(KnownWaveProperties.WaveHeight);
            block.Directions = boundaryCategory.GetDoubleValues(KnownWaveProperties.Direction);
            block.Periods = boundaryCategory.GetDoubleValues(KnownWaveProperties.Period);
            block.DirectionalSpreadings = boundaryCategory.GetDoubleValues(KnownWaveProperties.DirectionalSpreadingValue);
        }

        private static double ToDouble(this string value) => double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);

        private static T GetEnumValue<T>(this DelftIniCategory category, string propertyName) => EnumUtils.GetEnumValueByDescription<T>(category.GetPropertyValue(propertyName));

        private static double GetDoubleValue(this DelftIniCategory category, string propertyName) => category.GetPropertyValue(propertyName, double.NaN.ToString(CultureInfo.InvariantCulture)).ToDouble();

        private static double[] GetDoubleValues(this DelftIniCategory category, string propertyName) => category.GetPropertyValues(propertyName).Select(ToDouble).ToArray();

        private static string[] GetStringValues(this DelftIniCategory category, string propertyName) => category.GetPropertyValues(propertyName).ToArray();
    }
}