using System.Data;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRiverWeir : ISobekStructureDefinition
    {

        public SobekRiverWeir()
        {
            PositiveReductionTable = new DataTable();
            PositiveReductionTable.Columns.Add("S",typeof(double));
            PositiveReductionTable.Columns.Add("R", typeof(double));
            
            NegativeReductionTable = new DataTable();
            NegativeReductionTable.Columns.Add("S", typeof(double));
            NegativeReductionTable.Columns.Add("R", typeof(double));
        }
        /// <summary>
        /// cl
        /// </summary>
        public float CrestLevel { get; set; }

        /// <summary>
        /// cw
        /// </summary>
        public float CrestWidth { get; set; }
        
        /// <summary>
        /// cs
        /// </summary>
        public int CrestShape{get; set;}

        /// <summary>
        /// po
        /// </summary>
        public float CorrectionCoefficientPos { get; set; }

        /// <summary>
        /// ps
        /// </summary>
        public float SubmergeLimitPos { get; set; }

        /// <summary>
        /// pt pr 
        /// </summary>
        public DataTable PositiveReductionTable { get; set; }

        /// <summary>
        /// no
        /// </summary>
        public float CorrectionCoefficientNeg { get; set; }

        /// <summary>
        /// ns
        /// </summary>
        public float SubmergeLimitNeg { get; set; }
        
        /// <summary>
        /// nt nr
        /// </summary>
        public DataTable NegativeReductionTable { get; set; }
    }
}