using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class WaterFlowModel1DOutputFileMetaData
    {
        public IList<DateTime> Times { get; private set; }
        public IList<LocationMetaData> Locations { get; private set; }
        public IList<TimeDependentVariableMetaData> TimeDependentVariables { get; private set; }

        public int NumTimes { get { return Times.Count; } }
        public int NumLocations { get { return Locations.Count; } }

        public WaterFlowModel1DOutputFileMetaData(IList<DateTime> times = null, IList<LocationMetaData> locations = null, IList<TimeDependentVariableMetaData> timeDependentVariables = null)
        {
            Times = times ?? new List<DateTime>();
            Locations = locations ?? new List<LocationMetaData>();
            TimeDependentVariables = timeDependentVariables ?? new List<TimeDependentVariableMetaData>();
        }
    }

    public class LocationMetaData
    {
        public string Id { get; private set; }
        public int BranchId { get; private set; }
        public double Chainage { get; private set; }
        public double XCoordinate { get; private set; }
        public double YCoordinate { get; private set; }

        public LocationMetaData(string id, int branchId, double chainage, double xCoordinate, double yCoordinate)
        {
            Id = id;
            BranchId = branchId;
            Chainage = chainage;
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
        }
    }
    
    public class TimeDependentVariableMetaData
    {
        public string Name { get; private set; }
        public string LongName { get; private set; }
        public string Unit { get; private set; }
        public AggregationOptions AggregationOption { get; private set; }

        public TimeDependentVariableMetaData(string name, string longName, string unit, AggregationOptions aggregationOption)
        {
            Name = name;
            LongName = longName;
            Unit = unit;
            AggregationOption = aggregationOption;
        }
    }
}
