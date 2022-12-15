using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts
{
    class HbvModelController: ConceptModelController<HbvData>
    {
        public override bool CanHandle(ElementSet elementSet)
        {
            return elementSet == ElementSet.HbvElmSet;
        }

        protected override void OnAddArea(IRainfallRunoffModel model, HbvData data, IList<ModelLink> links)
        {
            links.Add(RainfallRunoffModelController.CreateModelLink(data.Catchment));

            var snowParameters = new[]
                {
                    data.SnowMeltingConstant, data.SnowFallTemperature, data.SnowMeltTemperature,
                    data.TemperatureAltitudeConstant, data.FreezingEfficiency, data.FreeWaterFraction
                };

            var soilParameters = new[] {data.Beta, data.FieldCapacity, data.FieldCapacityThreshold};

            var flowParameters = new[]
                {
                    data.BaseFlowReservoirConstant, data.InterflowReservoirConstant, data.QuickFlowReservoirConstant,
                    data.UpperZoneThreshold, data.MaximumPercolation
                };

            var hiniParameters = new[]
                {
                    data.InitialDrySnowContent, data.InitialFreeWaterContent, data.InitialSoilMoistureContents,
                    data.InitialUpperZoneContent, data.InitialLowerZoneContent
                };

            Writer.AddHbv(data.Catchment.Name, data.CalculationArea, data.SurfaceLevel, snowParameters, soilParameters,
                            flowParameters, hiniParameters, data.MeteoStationName, data.AreaAdjustmentFactor,
                            data.TemperatureStationName, data.Catchment?.InteriorPoint?.X ?? 0d, data.Catchment?.InteriorPoint?.Y ?? 0d);
        }
    }
}
