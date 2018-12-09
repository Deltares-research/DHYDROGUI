using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public interface IWaterFlowModelCategoryPropertySetter
    {
        void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages);
    }
}