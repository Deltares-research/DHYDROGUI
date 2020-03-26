using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Visitor used for writing the timeseries.bcw file.
    /// </summary>
    public class CollectFunctionsOfBoundariesDataComponentVisitor : BaseDataComponentVisitor
    {
        public readonly List<IFunction> listOfFunctions= new List<IFunction>();
        
        /// <summary>
        /// Visit method useful for retrieving all time series functions inside a boundary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="timeDependentParameters"></param>
        public override void Visit<T>(TimeDependentParameters<T> timeDependentParameters) 
        {
            listOfFunctions.Add(timeDependentParameters.WaveEnergyFunction.UnderlyingFunction);
        }
    }
}