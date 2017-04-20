using System;
using System.Collections.Generic;
using System.Data;
using DelftTools.Functions.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekController : IEquatable<SobekController>
    {
        public string Id;
        public string Name;
        public SobekControllerType ControllerType;
        public SobekControllerParameter SobekControllerParameterType;
        public bool IsActive;
        public string StructureId;
        public string MeasurementStationId;
        public SobekMeasurementLocationParameter MeasurementLocationParameter;
        public DataTable TimeTable;
        public DataTable LookUpTable;
        public InterpolationType InterpolationType;
        public ExtrapolationType ExtrapolationType;
        public string ExtrapolationPeriod;
        public double MaxChangeVelocity;
        public ISobekControllerProperties SpecificProperties;
        public IList<Trigger> Triggers { get; set; }
        public double PositiveStream;
        public double NegativeStream;
        public int MinimumPeriod;

        public static DataTable TimeTableStructure
        {
            get
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("time", typeof(DateTime));
                dataTable.Columns.Add("value", typeof(double));
                return dataTable;
            }
        }

        public static DataTable LookUpTableStructure
        {
            get
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("input", typeof(double));
                dataTable.Columns.Add("value", typeof(double));
                return dataTable;
            }
        }

        public bool Equals(SobekController other)
        {
            if (Object.ReferenceEquals(other, null)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            int hashId = Id == null ? 0 : Id.GetHashCode();
            return hashId;
        }

    }

    public enum SobekControllerType
    {
        TimeController = 0,
        HydraulicController = 1,
        IntervalController = 2,
        PIDController = 3,
        RelativeTimeController = 4,
        RelativeFromValueController = 5
    }

    public enum SobekControllerParameter
    {
        CrestLevel = 0,
        CrestWidth = 1,
        GateHeight = 2,
        PumpCapacity = 3,
        BottomLevel2DGridCell = 5
    }

    public enum SobekMeasurementLocationParameter
    {
        WaterLevel = 0,
        Discharge = 1,
        HeadDifference = 2,
        Velocity = 3,
        FlowDirection = 4,
        PressureDifference = 5
    }

    public struct Trigger
    {
        public string Id;
        public bool Active;
        public bool And;
    }
}
