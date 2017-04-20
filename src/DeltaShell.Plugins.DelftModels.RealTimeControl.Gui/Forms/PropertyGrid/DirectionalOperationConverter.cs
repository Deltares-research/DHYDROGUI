using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Forms.PropertyGrid
{
    public class DirectionalOperationConverter : OperationConverter
    {
        public DirectionalOperationConverter()
        {
            ConversionTable = new Dictionary<string, Operation>
                                  {
                                      {"Increasing", Operation.Greater},
                                      {"Increasing or unchanged", Operation.GreaterEqual},
                                      {"Unchanged", Operation.Equal},
                                      {"Changed", Operation.Unequal},
                                      {"Decreasing or unchanged", Operation.LessEqual},
                                      {"Decreasing", Operation.Less}
                                  };
        }
    }
}