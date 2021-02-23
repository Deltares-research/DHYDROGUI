using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;

namespace DelftTools.Hydro.Area.Objects.StructureObjects
{
    /// <summary>
    /// <see cref="IStructure"/> defines a single 2D structure.
    /// </summary>
    /// <seealso cref="IStructureObject" />
    /// <remarks>
    /// Deriving classes of <see cref="IStructure"/> are expected to have
    /// a <see cref="Utils.Aop.EntityAttribute"/>.
    /// </remarks>
    public interface IStructure : IStructureObject
    {
        /// <summary>
        /// Gets or sets the weir formula.
        /// </summary>
        IStructureFormula Formula { get; set; }

        /// <summary>
        /// Crest width (-1 : look at profile)
        /// </summary>
        double CrestWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="CrestLevelTimeSeries"/> or
        /// <see cref="CrestLevel"/> should be used.
        /// </summary>
        bool UseCrestLevelTimeSeries { get; set; }

        /// <summary>
        /// Gets or sets the crest level.
        /// </summary>
        double CrestLevel { get; set; }

        /// <summary>
        /// Gets the crest level time series.
        /// </summary>
        TimeSeries CrestLevelTimeSeries { get; }
    }
}