using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelNumericalParametersSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory numericalParametercategory, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            if (numericalParametercategory?.Name != ModelDefinitionsRegion.NumericalParametersValuesHeader) return;

            var errorMessages = new List<string>();

            foreach (var prop in numericalParametercategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == prop.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add(string.Format(
                        "Parameter {0} found in the md1d file. This parameter will not be imported, since it is not supported by the GUI",
                        prop.Name));
                    continue;
                }

                modelParameter.Value = prop.Value;
            }

            if (errorMessages.Count > 0)
            {
                createAndAddErrorReport?.Invoke(
                    "An error occurred during reading the numerical parameters of the md1d file:", errorMessages);
            }
        }
    }
}