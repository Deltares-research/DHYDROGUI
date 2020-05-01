using System;
using System.Globalization;
using System.IO;
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
        /// <param name="boundaryCategory">The boundary delft ini category.</param>
        /// <param name="mdwDirPath">The path to the directory where the .mdw file is located.</param>
        /// <returns>
        /// The created boundary mdw block.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="boundaryCategory"/> is not an mdw boundary category.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when the <paramref name="boundaryCategory"/> contains properties without a valid
        /// enum equivalent.
        /// </exception>
        public static BoundaryMdwBlock Convert(DelftIniCategory boundaryCategory, string mdwDirPath)
        {
            Ensure.NotNull(boundaryCategory, nameof(boundaryCategory));
            Ensure.NotNull(mdwDirPath, nameof(mdwDirPath));

            if (boundaryCategory.Name != KnownWaveCategories.BoundaryCategory)
            {
                throw new ArgumentException("Category is not an mdw boundary category.", nameof(boundaryCategory));
            }

            var block = new BoundaryMdwBlock {DefinitionType = boundaryCategory.GetEnumValue<DefinitionImportType>(KnownWaveProperties.Definition)};

            if (block.DefinitionType == DefinitionImportType.Oriented)
            {
                block.OrientationType = boundaryCategory.GetEnumValue<BoundaryOrientationType>(KnownWaveProperties.Orientation);

                string distanceDirType = boundaryCategory.GetPropertyValue(KnownWaveProperties.DistanceDir);
                block.DistanceDirType = distanceDirType != null
                                            ? EnumUtils.GetEnumValueByDescription<DistanceDirType>(distanceDirType)
                                            : DistanceDirType.CounterClockwise;
            }

            block.Name = boundaryCategory.GetPropertyValue(KnownWaveProperties.Name);
            block.XStartCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.StartCoordinateX);
            block.YStartCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.StartCoordinateY);
            block.XEndCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.EndCoordinateX);
            block.YEndCoordinate = boundaryCategory.GetDoubleValue(KnownWaveProperties.EndCoordinateY);
            block.SpectrumType = boundaryCategory.GetEnumValue<SpectrumImportExportType>(KnownWaveProperties.SpectrumSpec);
            block.Distances = boundaryCategory.GetDoubleValues(KnownWaveProperties.CondSpecAtDist);

            if (block.SpectrumType == SpectrumImportExportType.Parametrized)
            {
                ConvertParameterizedProperties(boundaryCategory, block);
            }
            else if (block.SpectrumType == SpectrumImportExportType.FromFile)
            {
                ConvertFileBasedProperties(boundaryCategory, block, mdwDirPath);
            }

            return block;
        }

        

        private static void ConvertFileBasedProperties(DelftIniCategory boundaryCategory, BoundaryMdwBlock block, string mdwDirPath)
        {
            block.SpectrumFiles = boundaryCategory.GetStringValues(KnownWaveProperties.Spectrum).Select(s => GetAbsolutePath(mdwDirPath, s)).ToArray();
        }

        private static string GetAbsolutePath(string mdwDirPath, string s)
        {
            return string.IsNullOrWhiteSpace(s) ? string.Empty : Path.Combine(mdwDirPath, s);
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