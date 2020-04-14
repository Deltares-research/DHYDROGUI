using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
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
        /// <param name="dataComponent">The uniform or spatially
        /// varying data component</param>
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

        private class Visitor : IDataComponentVisitor, IParametersVisitor
        {
            public List<IFunction> TimeSeries { get; } = new List<IFunction>();
            
            /// <summary>
            /// The collector needs to call the next AcceptVisitor method of the Data stored
            /// in the <see cref="UniformDataComponent{T}"/> object.
            /// </summary>
            /// <typeparam name="T"> An <see cref="IBoundaryConditionParameters"/> object</typeparam>
            /// <param name="uniformDataComponent">The visited <see cref="UniformDataComponent{T}"/></param>
            public void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IBoundaryConditionParameters
            {
                uniformDataComponent.Data.AcceptVisitor(this);
            }

            /// <summary>
            /// The collector needs to call the next AcceptVisitor methods of the stored data
            /// for all support points in the <see cref="SpatiallyVaryingDataComponent{T}"/> object.
            /// </summary>
            /// <typeparam name="T"> An <see cref="IBoundaryConditionParameters"/> object</typeparam>
            /// <param name="spatiallyVaryingDataComponent">The visited <see cref="SpatiallyVaryingDataComponent{T}"/></param>
            public void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IBoundaryConditionParameters
            {
                IOrderedEnumerable<KeyValuePair<SupportPoint, T>> sortedDictionary = spatiallyVaryingDataComponent.Data.OrderBy(kvp => kvp.Key.Distance);

                foreach (KeyValuePair<SupportPoint, T> supportPointKeyValuePair in sortedDictionary)
                {
                    supportPointKeyValuePair.Value.AcceptVisitor(this);
                }
            }

            /// <summary>
            /// The collector should add the retrieved time serie to <see cref="TimeSeries"/>,
            /// since it is a time dependent boundary.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="timeDependentParameters"></param>
            public void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new()
            {
                TimeSeries.Add(timeDependentParameters.WaveEnergyFunction.UnderlyingFunction);
            }

            /// <summary>
            /// The collector should do nothing, since it is apparently not a time dependent boundary. 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="constantParameters"></param>
            public void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new()
            {
                // Do nothing, since it is not a time dependent boundary.
            }
        }
    }
}