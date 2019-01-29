using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition {
    public static class DelftIniPropertyValidation
    {
        public static Dictionary<string, List<Tuple<string, bool, string>>> LookupTable =
            new Dictionary<string, List<Tuple<string, bool, string>>>
            {
                {
                    ModelDefinitionsRegion.TransportComputationValuesHeader, new List<Tuple<string, bool, string>>
                    {
                        new Tuple<string, bool, string>(ModelDefinitionsRegion.Density.Key, true,
                            DensityType.eckart_modified.ToString()),
                        new Tuple<string, bool, string>(ModelDefinitionsRegion.HeatTransferModel.Key, true,
                            TemperatureModelType.Transport.ToString()),
                        new Tuple<string, bool, string>(ModelDefinitionsRegion.UseTemperature.Key, true, "0"),
                    }
                },

                // todo: other validation objects to be configured here : See Issue ....
            };
    }
}