using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Converters
{
    public class OperationConverter : StringConverter
    {
        protected IDictionary<string, Operation> ConversionTable;

        public OperationConverter()
        {
            ConversionTable = new Dictionary<string, Operation>
            {
                {">", Operation.Greater},
                {">=", Operation.GreaterEqual},
                {"=", Operation.Equal},
                {"<>", Operation.Unequal},
                {"<=", Operation.LessEqual},
                {"<", Operation.Less}
            };
        }

        public string OperationToString(Operation operation)
        {
            return ConversionTable.Keys.Where(k => ConversionTable[k] == operation).FirstOrDefault();
        }

        public Operation StringToOperation(string value)
        {
            return ConversionTable[value];
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //true means show a combobox
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //true will limit to list. false will show the list, but allow free-form entry
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(ConversionTable.Keys.ToArray());
        }
    }
}