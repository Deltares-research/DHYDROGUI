using System.Data;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    /// <summary>
    /// Simple class to hold data read form Sobek struct.def file.
    /// Record types supported are
    /// ty 9 = Pump
    /// ty 3 = River Pump
    /// Pump and River Pump are identical except:
    ///   RiverPump does not explicitly support combined control direction (3 = 1 + 2 = upward + downward)
    /// </summary>
    public class SobekPump : ISobekStructureDefinition
    {
        public int Direction { get; set; }
        public DataTable ReductionTable { get; set; }
        public DataTable CapacityTable { get; set; }

        public SobekPump()
        {
            ReductionTable = ReductionTableStructure;
            CapacityTable = CapacityTableStructure;
        }

        private static DataTable ReductionTableStructure
        { 
            get
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("water level difference", typeof (double));
                dataTable.Columns.Add("reduction factor for capacity", typeof (double));
                return dataTable;
            }
        }

        private static DataTable CapacityTableStructure
        { 
            get
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("extra capacity", typeof (double));
                dataTable.Columns.Add("start level suction side", typeof (double));
                dataTable.Columns.Add("stop level suction side", typeof (double));
                dataTable.Columns.Add("start level pressure side", typeof (double));
                dataTable.Columns.Add("stop level pressure side", typeof (double));
                return dataTable;
            }
        }
    }
}