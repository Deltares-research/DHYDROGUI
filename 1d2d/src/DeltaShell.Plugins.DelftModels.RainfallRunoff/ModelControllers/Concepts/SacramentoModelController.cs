using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts
{
    class SacramentoModelController: ConceptModelController<SacramentoData>
    {
        public override bool CanHandle(ElementSet elementSet)
        {
            return elementSet == ElementSet.SacramentoElmSet;
        }

        protected override void OnAddArea(IRainfallRunoffModel model, SacramentoData data, IList<ModelLink> links)
        {
            links.Add(RainfallRunoffModelController.CreateModelLink(data.Catchment));

            var areaParameters = new[]
                {
                    data.PercolationIncrease, data.PercolationExponent, data.PercolatedWaterFraction,
                    data.FreeWaterFraction, data.PermanentlyImperviousFraction, data.RainfallImperviousFraction,
                    data.WaterAndVegetationAreaFraction, data.RatioUnobservedToObservedBaseFlow, data.SubSurfaceOutflow, data.TimeIntervalIncrement,
                    data.LowerRainfallThreshold, data.UpperRainfallThreshold
                };

            var capacities = new[]
                {
                    data.UpperZoneTensionWaterStorageCapacity, data.UpperZoneTensionWaterInitialContent,
                    data.UpperZoneFreeWaterStorageCapacity, data.UpperZoneFreeWaterInitialContent,
                    data.LowerZoneTensionWaterStorageCapacity, data.LowerZoneTensionWaterInitialContent,
                    data.LowerZoneSupplementalFreeWaterStorageCapacity,
                    data.LowerZoneSupplementalFreeWaterInitialContent, data.LowerZonePrimaryFreeWaterStorageCapacity,
                    data.LowerZonePrimaryFreeWaterInitialContent, data.UpperZoneFreeWaterDrainageRate,
                    data.LowerZoneSupplementalFreeWaterDrainageRate, data.LowerZonePrimaryFreeWaterDrainageRate
                };

            var hydrographValues = new double[36];
            data.HydrographValues.CopyTo(hydrographValues, 0);

            Writer.AddSacramento(data.Catchment.Name, data.CalculationArea, areaParameters, capacities,
                                   data.HydrographStep, hydrographValues, data.MeteoStationName, data.Catchment?.InteriorPoint?.X ?? 0d, data.Catchment?.InteriorPoint?.Y ?? 0d);
        }
    }
}
