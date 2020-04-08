using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Visitor used for writing the TimeSeries.bcw file.
    /// </summary>
    public class BcwTimeSeriesOfBoundaryCollector : IDataComponentVisitor, IParametersVisitor
    {
        private List<IFunction> TimeSeries { get; } = new List<IFunction>();


        public void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IBoundaryConditionParameters 
        { 
            uniformDataComponent.Data.AcceptVisitor(this);
        }

        public void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IBoundaryConditionParameters
        {
            IOrderedEnumerable<KeyValuePair<SupportPoint, T>> sortedDictionary = spatiallyVaryingDataComponent.Data.OrderBy(kvp => kvp.Key.Distance);

            foreach (KeyValuePair<SupportPoint, T> supportPointKeyValuePair in sortedDictionary)
            {
                supportPointKeyValuePair.Value.AcceptVisitor(this);
            }
        }

        /// <summary>
        /// Visit method for adding time series. Will be called for every support point
        /// of spatially varying boundaries. For uniform boundaries, this method will be
        /// called once.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="timeDependentParameters"></param>
        public void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new() 
        {
            TimeSeries.Add(timeDependentParameters.WaveEnergyFunction.UnderlyingFunction);
        }

        public void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new() {}

/// <summary>
/// Collect all time series functions inside a data component.
/// </summary>
/// <param name="dataComponent"></param>
/// <returns></returns>
public static List<IFunction> Collect(IBoundaryConditionDataComponent dataComponent)
        {
            var visitor = new BcwTimeSeriesOfBoundaryCollector();
            dataComponent.AcceptVisitor(visitor);

            return visitor.TimeSeries;
        }
    }
}