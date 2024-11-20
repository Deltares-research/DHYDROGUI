using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class OutputFile1DMetaData
    {
        public IList<DateTime> Times { get; private set; }
        public IDictionary<TimeDependentVariableMetaDataBase, IList<LocationMetaData>> Locations { get; private set; }
        public IList<TimeDependentVariableMetaDataBase> TimeDependentVariables { get; private set; }

        public OutputFile1DMetaData(IList<DateTime> times = null, IDictionary<TimeDependentVariableMetaDataBase, IList<LocationMetaData>> locations = null, IList<TimeDependentVariableMetaDataBase> timeDependentVariables = null) 
        {
            Times = times ?? new List<DateTime>();
            Locations = locations ?? new Dictionary<TimeDependentVariableMetaDataBase, IList<LocationMetaData>>();
            TimeDependentVariables = timeDependentVariables ?? new List<TimeDependentVariableMetaDataBase>();
        }

        public int NumLocationsForFunctionId(string getNetCdfVariableName)
        {
            return string.IsNullOrEmpty(getNetCdfVariableName) || !Locations.Any(l => l.Key.Name.Equals(getNetCdfVariableName, StringComparison.InvariantCultureIgnoreCase)) 
                ? 0 
                : Locations.FirstOrDefault(l => l.Key.Name.Equals(getNetCdfVariableName, StringComparison.InvariantCultureIgnoreCase)).Value.Count;
        }
    }
}