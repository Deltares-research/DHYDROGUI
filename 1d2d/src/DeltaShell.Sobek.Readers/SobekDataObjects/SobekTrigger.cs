using System;
using System.Data;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekTrigger : IEquatable<SobekTrigger>
    {
        public string Id;
        public string Name;
        public string MeasurementStationId;
        public string StructureId;
        public bool OnceHydraulicTrigger;
        public SobekTriggerType TriggerType;
        public SobekTriggerParameterType TriggerParameterType;
        public SobekTriggerCheckOn  CheckOn;
        public DataTable TriggerTable;

        /// <summary>
        /// Period in seconds
        /// </summary>
        public string PeriodicExtrapolationPeriod;

        public static DataTable TriggerTableStructure{
            get
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("Time", typeof(DateTime));
                    dataTable.Columns.Add("OnOff", typeof(bool));
                    dataTable.Columns.Add("AndOr", typeof(bool));
                    dataTable.Columns.Add("Operation", typeof(bool)); //false = LessThen, true = GreaterThen
                    dataTable.Columns.Add("Value", typeof(double));
                    return dataTable;
                }
        }

        public bool Equals(SobekTrigger other)
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

    public enum SobekTriggerType
    {
        Time = 0,
        Hydraulic = 1,
        TimeAndHydraulic = 2
    }

    public enum SobekTriggerParameterType
    {
        WaterLevelBranchLocation = 0,   // observation point
        HeadDifferenceStructure = 1,    // structure
        DischargeBranchLocation = 2,    // observation point
        GateHeightStructure = 3,        // structure
        CrestLevelStructure = 4,        // structure
        CrestWidthStructure = 5,        // structure
        WaterlevelRetentionArea = 6,    // retention area
        PressureDifferenceStructure = 7 // structure
    }

    public enum SobekTriggerCheckOn
    {
        Value = 0,
        Direction = 1
    }
}
