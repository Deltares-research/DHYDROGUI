using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Utilities;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Converter for converting a boundary mdw section to a <see cref="BoundaryMdwBlock"/>.
    /// </summary>
    public static class BoundarySectionConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="boundarySection"/>
        /// to a <see cref="BoundaryMdwBlock"/>.
        /// </summary>
        /// <param name="boundarySection">The boundary INI section.</param>
        /// <param name="mdwDirPath">The path to the directory where the .mdw file is located.</param>
        /// <returns>
        /// The created boundary mdw block.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="boundarySection"/> is not an mdw boundary section.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when the <paramref name="boundarySection"/> contains properties without a valid
        /// enum equivalent.
        /// </exception>
        public static BoundaryMdwBlock Convert(IniSection boundarySection, string mdwDirPath)
        {
            Ensure.NotNull(boundarySection, nameof(boundarySection));
            Ensure.NotNull(mdwDirPath, nameof(mdwDirPath));

            if (boundarySection.Name != KnownWaveSections.BoundarySection)
            {
                throw new ArgumentException("Section is not an mdw boundary section.", nameof(boundarySection));
            }

            var block = new BoundaryMdwBlock {DefinitionType = boundarySection.GetEnumValue<DefinitionImportType>(KnownWaveProperties.Definition)};

            if (block.DefinitionType == DefinitionImportType.Oriented)
            {
                block.OrientationType = boundarySection.GetEnumValue<BoundaryOrientationType>(KnownWaveProperties.Orientation);

                string distanceDirType = boundarySection.GetPropertyValue(KnownWaveProperties.DistanceDir);
                block.DistanceDirType = distanceDirType != null
                                            ? EnumUtils.GetEnumValueByDescription<DistanceDirType>(distanceDirType)
                                            : DistanceDirType.CounterClockwise;
            }

            block.Name = boundarySection.GetPropertyValue(KnownWaveProperties.Name);
            block.XStartCoordinate = boundarySection.GetDoubleValue(KnownWaveProperties.StartCoordinateX).Round();
            block.YStartCoordinate = boundarySection.GetDoubleValue(KnownWaveProperties.StartCoordinateY).Round();
            block.XEndCoordinate = boundarySection.GetDoubleValue(KnownWaveProperties.EndCoordinateX).Round();
            block.YEndCoordinate = boundarySection.GetDoubleValue(KnownWaveProperties.EndCoordinateY).Round();
            block.SpectrumType = boundarySection.GetEnumValue<SpectrumImportExportType>(KnownWaveProperties.SpectrumSpec);
            block.Distances = boundarySection.GetDoubleValues(KnownWaveProperties.CondSpecAtDist).Select(Round).ToArray();

            if (block.SpectrumType == SpectrumImportExportType.Parametrized)
            {
                ConvertParameterizedProperties(boundarySection, block);
            }
            else if (block.SpectrumType == SpectrumImportExportType.FromFile)
            {
                ConvertFileBasedProperties(boundarySection, block, mdwDirPath);
            }

            return block;
        }

        private static void ConvertFileBasedProperties(IniSection boundarySection, BoundaryMdwBlock block, string mdwDirPath)
        {
            block.SpectrumFiles = boundarySection.GetStringValues(KnownWaveProperties.Spectrum).Select(s => GetAbsolutePath(mdwDirPath, s)).ToArray();
        }

        private static string GetAbsolutePath(string mdwDirPath, string s)
        {
            return string.IsNullOrWhiteSpace(s) ? string.Empty : Path.Combine(mdwDirPath, s);
        }

        private static void ConvertParameterizedProperties(IniSection boundarySection, BoundaryMdwBlock block)
        {
            block.ShapeType = boundarySection.GetEnumValue<ShapeImportType>(KnownWaveProperties.ShapeType);
            block.PeriodType = boundarySection.GetEnumValue<PeriodImportExportType>(KnownWaveProperties.PeriodType);
            block.SpreadingType = boundarySection.GetEnumValue<SpreadingImportType>(KnownWaveProperties.DirectionalSpreadingType);
            block.PeakEnhancementFactor = boundarySection.GetDoubleValue(KnownWaveProperties.PeakEnhancementFactor);
            block.Spreading = boundarySection.GetDoubleValue(KnownWaveProperties.GaussianSpreading);
            block.WaveHeights = boundarySection.GetDoubleValues(KnownWaveProperties.WaveHeight);
            block.Directions = boundarySection.GetDoubleValues(KnownWaveProperties.Direction);
            block.Periods = boundarySection.GetDoubleValues(KnownWaveProperties.Period);
            block.DirectionalSpreadings = boundarySection.GetDoubleValues(KnownWaveProperties.DirectionalSpreadingValue);
        }

        private static double ToDouble(this string value) => double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);

        private static double Round(this double value) => SpatialDouble.Round(value);

        private static T GetEnumValue<T>(this IniSection section, string propertyKey) => EnumUtils.GetEnumValueByDescription<T>(section.GetPropertyValue(propertyKey));

        private static double GetDoubleValue(this IniSection section, string propertyKey) => section.GetPropertyValue(propertyKey, double.NaN);

        private static double[] GetDoubleValues(this IniSection section, string propertyKey) => section.GetAllProperties(propertyKey).Select(p => p.Value).Select(ToDouble).ToArray();

        private static string[] GetStringValues(this IniSection section, string propertyKey) => section.GetAllProperties(propertyKey).Select(p => p.Value).ToArray();
    }
}