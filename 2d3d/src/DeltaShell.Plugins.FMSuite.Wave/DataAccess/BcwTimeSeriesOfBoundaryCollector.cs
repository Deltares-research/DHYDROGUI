using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// Static class containing static method to start collecting time series
    /// for writing them in the TimeSeries.bcw file.
    /// This class has a visitor as private nested class, since the visitor
    /// must only be used in this context.
    /// </summary>
    public static class BcwTimeSeriesOfBoundaryCollector
    {
        /// <summary>
        /// Collect all time series functions inside a data component.
        /// </summary>
        /// <param name="dataComponent">
        /// The uniform or spatially
        /// varying data component
        /// </param>
        /// <returns>List of functions of a boundary</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataComponent"/>
        /// is <c>null</c>.
        /// </exception>
        public static List<IFunction> Collect(ISpatiallyDefinedDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));

            var visitor = new Visitor();
            dataComponent.AcceptVisitor(visitor);

            return visitor.TimeSeries;
        }

        private class Visitor : ISpatiallyDefinedDataComponentVisitor, IForcingTypeDefinedParametersVisitor
        {
            public List<IFunction> TimeSeries { get; } = new List<IFunction>();

            /// <summary>
            /// The collector should add the retrieved time series to <see cref="TimeSeries"/>,
            /// since it is a time dependent boundary.
            /// </summary>
            /// <typeparam name="T">The type of spreading.</typeparam>
            /// <param name="timeDependentParameters">The visited <see cref="TimeDependentParameters{TSpreading}"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="timeDependentParameters"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new()
            {
                Ensure.NotNull(timeDependentParameters, nameof(timeDependentParameters));
                TimeSeries.Add(timeDependentParameters.WaveEnergyFunction.UnderlyingFunction);
            }

            /// <summary>
            /// The collector should do nothing, since it is apparently not a time dependent boundary.
            /// </summary>
            /// <param name="fileBasedParameters">The visited file based parameters.</param>
            public void Visit(FileBasedParameters fileBasedParameters)
            {
                // Do nothing, since it is not a time dependent boundary.
            }

            /// <summary>
            /// The collector should do nothing, since it is apparently not a time dependent boundary.
            /// </summary>
            /// <typeparam name="T">The type of spreading.</typeparam>
            /// <param name="constantParameters">The visited constant parameters.</param>
            public void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new()
            {
                // Do nothing, since it is not a time dependent boundary.
            }

            /// <summary>
            /// The collector needs to call the next AcceptVisitor method of the Data stored
            /// in the <see cref="UniformDataComponent{T}"/> object.
            /// </summary>
            /// <typeparam name="T"> An <see cref="IForcingTypeDefinedParameters"/> object</typeparam>
            /// <param name="uniformDataComponent">The visited <see cref="UniformDataComponent{T}"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="uniformDataComponent"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IForcingTypeDefinedParameters
            {
                Ensure.NotNull(uniformDataComponent, nameof(uniformDataComponent));
                uniformDataComponent.Data.AcceptVisitor(this);
            }

            /// <summary>
            /// The collector needs to call the next AcceptVisitor methods of the stored data
            /// for all support points in the <see cref="SpatiallyVaryingDataComponent{T}"/> object.
            /// </summary>
            /// <typeparam name="T"> An <see cref="IForcingTypeDefinedParameters"/> object</typeparam>
            /// <param name="spatiallyVaryingDataComponent">The visited <see cref="SpatiallyVaryingDataComponent{T}"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="spatiallyVaryingDataComponent"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IForcingTypeDefinedParameters
            {
                Ensure.NotNull(spatiallyVaryingDataComponent, nameof(spatiallyVaryingDataComponent));
                IOrderedEnumerable<KeyValuePair<SupportPoint, T>> sortedDictionary = spatiallyVaryingDataComponent.Data.OrderBy(kvp => kvp.Key.Distance);

                foreach (KeyValuePair<SupportPoint, T> supportPointKeyValuePair in sortedDictionary)
                {
                    supportPointKeyValuePair.Value.AcceptVisitor(this);
                }
            }
        }
    }
}