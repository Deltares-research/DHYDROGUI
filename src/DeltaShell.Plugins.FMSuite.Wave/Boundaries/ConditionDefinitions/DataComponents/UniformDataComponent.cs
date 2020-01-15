using System;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents
{
    /// <summary>
    /// <see cref="UniformDataComponent"/> defines a data component consisting
    /// of a uniform data object for all support points.
    /// </summary>
    /// <seealso cref="IBoundaryConditionDataComponent" />
    public class UniformDataComponent : IBoundaryConditionDataComponent
    {
        /// <summary>
        /// Creates a new <see cref="UniformDataComponent"/>.
        /// </summary>
        /// <param name="data">The data of this .</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        // TODO (MWT) Verify whether we should add an Extension method to verify the IBoundaryConditionParameters as being valid
        public UniformDataComponent(IBoundaryConditionParameters data)
        {
            Ensure.NotNull(data, nameof(data));
            Data = data;
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public IBoundaryConditionParameters Data
        {
            get => data;
            set
            {
                Ensure.NotNull(value, nameof(value));
                data = value;
            }
        }

        private IBoundaryConditionParameters data;
    }
}