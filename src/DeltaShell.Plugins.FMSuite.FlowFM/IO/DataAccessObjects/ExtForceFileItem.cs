using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public class ExtForceFileItem
    {
        public ExtForceFileItem(string quantity)
        {
            ModelData = new Dictionary<string, object>();

            Quantity = quantity;
            Enabled = true;
            FileType = int.MinValue;
            Method = int.MinValue;

            // optional additional data
            Value = double.NaN;
            Factor = double.NaN;
            Offset = double.NaN;
        }

        // general data
        public string Quantity { get; set; }
        public string FileName { get; set; }
        
        /// <summary>
        /// Optional property for variable name of data set
        /// Note: not used in the GUI
        /// </summary>
        public string VarName { get; set; }
        public int FileType { get; set; }
        public int Method { get; set; }
        public string Operand { get; set; }

        public bool Enabled { get; set; }

        // optional additional data for polygon(s) in pol file (Double.NaN if not specified)
        public double Value { get; set; }
        public double Factor { get; set; }
        public double Offset { get; set; }

        // optional additional data, e.g. for friction type
        public Dictionary<string, object> ModelData { get; set; }

    }
}