using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.NGHS.IO.Store1D
{
    public class OutputFile1DMetaData<U> where U : ITimeDependentVariableMetaDataBase, new()
    {
        public IList<DateTime> Times { get; private set; }
        public IDictionary<U, IList<LocationMetaData>> Locations { get; private set; }
        public IList<U> TimeDependentVariables { get; private set; }

        public int NumTimes { get { return Times.Count; } }
        
        public OutputFile1DMetaData(IList<DateTime> times = null, IDictionary<U, IList<LocationMetaData>> locations = null, IList<U> timeDependentVariables = null) 
        {
            Times = times ?? new List<DateTime>();
            Locations = locations ?? new Dictionary<U, IList<LocationMetaData>>();
            TimeDependentVariables = timeDependentVariables ?? new List<U>();
        }

        public int NumLocationsForFunctionId(string getNetCdfVariableName)
        {
            return Locations.FirstOrDefault(l => l.Key.Name == getNetCdfVariableName).Value.Count;
        }
    }
}