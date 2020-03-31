using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Visitor used for writing the TimeSeries.bcw file.
    /// </summary>
    public class BcwTimeSeriesOfBoundaryCollector : BaseDataComponentVisitor
    {
        private List<IFunction> TimeSeries { get; } = new List<IFunction>();
        
        /// <summary>
        /// Visit method for adding time series. Will be called for every support point
        /// of spatially varying boundaries. For uniform boundaries, this method will be
        /// called once.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="timeDependentParameters"></param>
        public override void Visit<T>(TimeDependentParameters<T> timeDependentParameters) 
        {
            TimeSeries.Add(timeDependentParameters.WaveEnergyFunction.UnderlyingFunction);
        }

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