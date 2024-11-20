using System.Data;
using DelftTools.Functions.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRSeepage
    {
        public SobekRRSeepage()
        {
            SaltConcentration = 500;
        }

        public string Id
        {
            set; 
            get;
        }

        public string Name
        {
            set;
            get;
        }

        public SeepageComputationOption ComputationOption
        {
            set;
            get;
        }

        /// <summary>
        /// Seepage or percolation  (mm/day)
        /// Positive numbers represent seepage, negative numbers represent percolation.
        /// </summary>
        public double Seepage
        {
            set;
            get;
        }

        /// <summary>
        /// (mg/l) Default 500 mg/l.
        /// </summary>
        public double SaltConcentration
        {
            set;
            get;
        }

        /// <summary>
        /// Resistance value C for aquitard 
        /// </summary>
        public double ResistanceValue
        {
            set;
            get;
        }

        public string H0TableName
        {
            set;
            get;
        }

        public InterpolationType InterpolationType
        {
            set;
            get;
        }

        public ExtrapolationType ExtrapolationType
        {
            set;
            get;
        }

        public string ExtrapolationPeriod
        {
            set;
            get;
        }

        public DataTable SaltTableConcentration
        {
            set;
            get;
        }

    }

    public enum SeepageComputationOption
    {
        Constant = 1,
        VariableH0 = 2,
        VariableModFlow = 3,
        TimeTable = 4,
        TimeTableAndSaltConcentration = 5
    }
}