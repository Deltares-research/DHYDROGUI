using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelObservationsParameterSetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory observationsParameterCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (observationsParameterCategory == null) return;
            if (!ValueEqualsDefinition(observationsParameterCategory.Name, ModelDefinitionsRegion.ObservationsHeader)) return;

            foreach (var property in observationsParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ValueEqualsDefinition(ps.Name, property.Name));
                if (modelParameter == null)
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                    continue;
                }

                modelParameter.Value = property.Value;
            }
        }
    }
}