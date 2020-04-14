using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents
{
    /// <summary>
    /// <see cref="UniformDataComponent{T}"/> defines a data component consisting
    /// of a <see cref="IBoundaryConditionParameters"/> defined for the whole
    /// <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="IBoundaryConditionParameters"/>.
    /// </typeparam>
    /// <seealso cref="ISpatiallyDefinedDataComponent" />
    public class UniformDataComponent<T> : ISpatiallyDefinedDataComponent where T : IBoundaryConditionParameters
    {
        /// <summary>
        /// Creates a new <see cref="UniformDataComponent{T}"/>.
        /// </summary>
        /// <param name="data">The data of this <see cref="UniformDataComponent{T}"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        public UniformDataComponent(T data)
        {
            Ensure.NotNull((IBoundaryConditionParameters) data, nameof(data));
            Data = data;
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public T Data
        {
            get => data;
            set
            {
                Ensure.NotNull((IBoundaryConditionParameters) value, nameof(value));
                data = value;
            }
        }

        private T data;

        public void AcceptVisitor(IDataComponentVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}