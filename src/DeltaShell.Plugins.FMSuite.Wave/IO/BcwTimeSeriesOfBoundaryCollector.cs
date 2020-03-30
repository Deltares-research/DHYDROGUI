using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Visitor used for writing the timeSeries.bcw file.
    /// </summary>
    public class BcwTimeSeriesOfBoundaryCollector : BaseDataComponentVisitor
    {
        private List<IFunction> timeSeries= new List<IFunction>();
        
        /// <summary>
        /// Visit method useful for retrieving all time series functions inside a boundary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="timeDependentParameters"></param>
        public override void Visit<T>(TimeDependentParameters<T> timeDependentParameters) 
        {
            timeSeries.Add(timeDependentParameters.WaveEnergyFunction.UnderlyingFunction);
        }

        public static List<IFunction> Collect(IWaveBoundary boundary)
        {
            var visitor = new BcwTimeSeriesOfBoundaryCollector();
            boundary.ConditionDefinition.DataComponent.AcceptVisitor(visitor);

            return visitor.timeSeries;
        }
    }
}