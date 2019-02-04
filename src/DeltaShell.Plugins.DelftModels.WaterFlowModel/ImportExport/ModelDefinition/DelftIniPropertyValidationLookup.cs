using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public static class DelftIniPropertyValidationLookup
    {
        /// <summary>
        /// Lookup table including validation objects with default value reference used for DelftIniProperty validation.
        /// <remarks>other validation objects to be configured here</remarks>
        /// </summary>
        public static readonly Dictionary<string, List<Tuple<string, bool, List<string>>>> LookupTable =
            new Dictionary<string, List<Tuple<string, bool, List<string>>>>
            {
                {
                    ModelDefinitionsRegion.TransportComputationValuesHeader, new List<Tuple<string, bool, List<string>>>
                    {
                        new Tuple<string, bool, List<string>>(ModelDefinitionsRegion.Density.Key, true,
                            new List<string>{DensityType.eckart_modified.ToString(),
                                             DensityType.eckart.ToString(),
                                             DensityType.unesco.ToString()

                            }),

                        new Tuple<string, bool, List<string>>(ModelDefinitionsRegion.HeatTransferModel.Key, true,
                            new List<string>{TemperatureModelType.Transport.ToString(),
                                             TemperatureModelType.Composite.ToString(),
                                             TemperatureModelType.Excess.ToString()

                            }),

                        new Tuple<string, bool, List<string>>(ModelDefinitionsRegion.UseTemperature.Key, true,
                            new List<string>{"0", "1"})
                    }
                },
            };
    }
}