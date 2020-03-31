using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
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
        /// Visit method useful for retrieving all time series functions inside a boundary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="timeDependentParameters"></param>
        public override void Visit<T>(TimeDependentParameters<T> timeDependentParameters) 
        {
            TimeSeries.Add(timeDependentParameters.WaveEnergyFunction.UnderlyingFunction);
        }

        public static List<IFunction> Collect(IWaveBoundary boundary)
        {
            var visitor = new BcwTimeSeriesOfBoundaryCollector();
            boundary.ConditionDefinition.DataComponent.AcceptVisitor(visitor);

            return visitor.TimeSeries;
        }
    }
}