using System;
using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents
{
    /// <summary>
    /// <see cref="SpatiallyVaryingDataComponent{T}"/> defines a data component consisting
    /// of an optional <see cref="IForcingTypeDefinedParameters"/> per support point.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="IForcingTypeDefinedParameters"/>.
    /// </typeparam>
    /// <seealso cref="ISpatiallyDefinedDataComponent"/>
    public class SpatiallyVaryingDataComponent<T> : ISpatiallyDefinedDataComponent where T : IForcingTypeDefinedParameters
    {
        private readonly Dictionary<SupportPoint, T> data =
            new Dictionary<SupportPoint, T>();

        /// <summary>
        /// Gets the dictionary containing the data.
        /// </summary>
        public IReadOnlyDictionary<SupportPoint, T> Data => data;

        /// <summary>
        /// Add the specified <paramref name="supportPoint"/> and corresponding
        /// <paramref name="parameters"/>.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <param name="parameters">The parameter data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="supportPoint"/> already exists within Data.
        /// </exception>
        public void AddParameters(SupportPoint supportPoint, T parameters)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));
            Ensure.NotNull((IForcingTypeDefinedParameters) parameters, nameof(parameters));

            if (data.ContainsKey(supportPoint))
            {
                throw new InvalidOperationException($"Support point: {supportPoint} already exists within Data.");
            }

            data[supportPoint] = parameters;
        }

        /// <summary>
        /// Removes the specified <paramref name="supportPoint"/>.
        /// </summary>
        /// <param name="supportPoint">The support point to remove.</param>
        public void RemoveSupportPoint(SupportPoint supportPoint) =>
            data.Remove(supportPoint);

        /// <summary>
        /// Replaces the <paramref name="oldSupportPoint"/> with the new
        /// <paramref name="newSupportPoint"/>.
        /// </summary>
        /// <param name="oldSupportPoint">The old support point.</param>
        /// <param name="newSupportPoint">The new support point.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="oldSupportPoint"/> does not exist within Data or
        /// when <paramref name="newSupportPoint"/> already exists within Data.
        /// </exception>
        public void ReplaceSupportPoint(SupportPoint oldSupportPoint,
                                        SupportPoint newSupportPoint)
        {
            Ensure.NotNull(oldSupportPoint, nameof(oldSupportPoint));
            Ensure.NotNull(newSupportPoint, nameof(newSupportPoint));

            if (!data.ContainsKey(oldSupportPoint))
            {
                throw new InvalidOperationException($"Support point: {oldSupportPoint} does not exist within Data.");
            }

            AddParameters(newSupportPoint, Data[oldSupportPoint]);
            RemoveSupportPoint(oldSupportPoint);
        }

        public void AcceptVisitor(ISpatiallyDefinedDataComponentVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}