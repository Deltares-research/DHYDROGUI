using System;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="DefinitionGeneratorTimeSeriesStructure"/> implements a base class for
    /// <see cref="DefinitionGeneratorStructure"/> with TimeSeries.
    /// </summary>
    public abstract class DefinitionGeneratorTimeSeriesStructure : DefinitionGeneratorStructure
    {
        private readonly IStructureFileNameGenerator structureFileNameGenerator;

        /// <summary>
        /// Creates a new <see cref="DefinitionGeneratorTimeSeriesStructure"/>
        /// </summary>
        /// <param name="structureFileNameGenerator">Generator to generate correct file name for structure.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="structureFileNameGenerator"/> is <c>null</c>. </exception>
        protected DefinitionGeneratorTimeSeriesStructure(IStructureFileNameGenerator structureFileNameGenerator)
        {
            Ensure.NotNull(structureFileNameGenerator, nameof(structureFileNameGenerator));
            this.structureFileNameGenerator = structureFileNameGenerator;
        }

        /// <summary>
        /// Add property to IniSection, either as time series or normally.
        /// </summary>
        /// <param name="isTimeSeries">Signifies property should be handled as time series.</param>
        /// <param name="key">Name for property.</param>
        /// <param name="value">Value of structure.</param>
        /// <param name="description">Comment, can be <c>null</c>.</param>
        /// <param name="format">Format for property.</param>
        protected void AddProperty(bool isTimeSeries, string key, double value,  string description, string format)
        {
            if (isTimeSeries)
            {
                IniSection.AddPropertyWithOptionalComment(key, structureFileNameGenerator.Generate(), description);
            }
            else
            {
                IniSection.AddPropertyWithOptionalCommentAndFormat(key, value, description, format);
            }
        }

        /// <summary>
        /// Add property to IniSection, either as time series or normally.
        /// </summary>
        /// <param name="isTimeSeries">Signifies property should be handled as time series.</param>
        /// <param name="propertyConfiguration">propertyConfiguration of structure</param>
        /// <param name="value">Value of structure.</param>
        protected void AddProperty(bool isTimeSeries, ConfigurationSetting propertyConfiguration, double value)
        {
            if (isTimeSeries)
            {
                IniSection.AddPropertyFromConfiguration(propertyConfiguration, structureFileNameGenerator.Generate());
            }
            else
            {
                IniSection.AddPropertyFromConfiguration(propertyConfiguration, value);
            }
        }
    }
}