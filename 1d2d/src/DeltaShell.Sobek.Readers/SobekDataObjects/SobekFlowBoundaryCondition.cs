using System;
using System.Data;
using DelftTools.Functions.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum SobekFlowBoundaryConditionType
    {
        Level =0,
        Flow = 1
    }

    public enum SobekFlowBoundaryStorageType
    {
        Constant = 0,
        Variable = 1,
        Fourier = 2,
        Tidal = 3,
        Qh = 4, // - Hq
        Librarytable = 11
    }

    public class SobekFlowBoundaryCondition
    {
        public SobekFlowBoundaryConditionType BoundaryType { get; set; }
        public SobekFlowBoundaryStorageType StorageType { get; set; }
        public string ID { get; set; }
        
        public double FlowConstant { get; set; }
        public double LevelConstant { get; set; }

        public DataTable LevelTimeTable { get; set; }
        public DataTable FlowTimeTable { get; set; }
        public DataTable FlowHqTable { get; set; }
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

        public static DataTable HqTableStructure
        {
            get
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("H", typeof(double));
                dataTable.Columns.Add("Q", typeof(double));
                return dataTable;
            }
        }

        public static DataTable QhTableStructure
        {
            get
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Q", typeof(double));
                dataTable.Columns.Add("H", typeof(double));
                return dataTable;
            }
        }

        public InterpolationType InterpolationType{get; set;}

        public ExtrapolationType ExtrapolationType { get; set; }

        public string ExtrapolationPeriod { get; set; }

    }
}