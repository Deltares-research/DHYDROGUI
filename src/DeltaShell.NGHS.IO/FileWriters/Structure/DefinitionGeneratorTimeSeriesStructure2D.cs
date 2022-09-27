using DelftTools.Functions;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="DefinitionGeneratorTimeSeriesStructure2D"/> implements a base class for
    /// <see cref="DefinitionGeneratorStructure2D"/> with TimeSeries.
    /// </summary>
    public abstract class DefinitionGeneratorTimeSeriesStructure2D : DefinitionGeneratorStructure2D
    {
        /// <summary>
        /// Add property to IniCategory, either as time series or normally.
        /// </summary>
        /// <param name="isTimeSeries">Signifies property should be handled as time series.</param>
        /// <param name="key">Name for property.</param>
        /// <param name="value">Value of structure.</param>
        /// <param name="description">Comment, can be <c>null</c>.</param>
        /// <param name="format">Format for property.</param>
        /// <param name="structure">Structure for which property is added.</param>
        /// <param name="timeSeries">Time series added.</param>
        protected void AddProperty(bool isTimeSeries, string key, double value, string description, string format, IStructure structure,
                                   ITimeSeries timeSeries)
        {
            if (isTimeSeries)
            {
                AddPropertyAsTimeSeries(key, description, structure, timeSeries);
            }
            else
            {
                IniCategory.AddProperty(key, value, description, format);
            }
        }

        /// <summary>
        /// Add property to IniCategory as time series.
        /// </summary>
        /// <param name="key">Name for property.</param>
        /// <param name="description">Comment, can be <c>null</c>.</param>
        /// <param name="structure">Structure for which property is added.</param>
        /// <param name="timeSeries">Time series added.</param>
        private void AddPropertyAsTimeSeries(string key, string description, IStructure structure,
                                             ITimeSeries timeSeries)
        {
            IniCategory.AddProperty(key,
                                    StructureTimFileNameGenerator.Generate(structure, timeSeries),
                                    description);
        }
    }
}