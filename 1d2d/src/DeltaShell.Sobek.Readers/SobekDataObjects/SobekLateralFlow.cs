using System;
using System.Data;
using DelftTools.Functions.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    /// <summary>
    /// For storage of the data found in LATERAL.DAT
    /// </summary>
    public class SobekLateralFlow
    {
        public SobekLateralFlow()
        {
            IsPointDischarge = true;
            IsConstantDischarge = true;
        }

        /// <summary>
        /// field id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// lt 0 
        /// = lenght of discharge is 0; this field pre
        /// </summary>
        public bool IsPointDischarge { get; set; }

        /// <summary>
        /// dc lt 0 = constant value 
        /// 
        /// </summary>
        public bool IsConstantDischarge { get; set; }
        public double ConstantDischarge { get; set; }

        public DataTable FlowTimeTable { get; set; }
        public DataTable LevelQhTable { get; set; }


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

        public InterpolationType InterpolationType { get; set; }

        public ExtrapolationType ExtrapolationType { get; set; }

        public string ExtrapolationPeriod { get; set; }

        public double Length { get; set; }
    }
}
