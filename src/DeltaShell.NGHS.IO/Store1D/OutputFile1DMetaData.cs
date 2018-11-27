using System;
using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Store1D
{
    public class OutputFile1DMetaData<T, U> where T : LocationMetaData, new() where U : ITimeDependentVariableMetaDataBase, new()
    {
        public IList<DateTime> Times { get; private set; }
        public IList<T> Locations { get; private set; }
        public IList<U> TimeDependentVariables { get; private set; }

        public int NumTimes { get { return Times.Count; } }
        public int NumLocations { get { return Locations.Count; } }

        public OutputFile1DMetaData(IList<DateTime> times = null, IList<T> locations = null, IList<U> timeDependentVariables = null) 
        {
            Times = times ?? new List<DateTime>();
            Locations = locations ?? new List<T>();
            TimeDependentVariables = timeDependentVariables ?? new List<U>();
        }
        
    }
}