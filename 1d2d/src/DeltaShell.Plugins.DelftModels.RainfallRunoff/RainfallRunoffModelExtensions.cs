using System;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using GeoAPI.Extensions.Coverages;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public static class RainfallRunoffModelExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (RainfallRunoffModel));

        public static bool SetInitialConditionsFromPreviousOutput(this RainfallRunoffModel model, DateTime outputTime)
        {
            // copy the 4 initial conditions from the output coverages if they are available

            SetInitialConditionsForQuantity<PavedData>(model, outputTime,
                GetOutputCoverageByEngineParameters(model, QuantityType.StorageStreet_mm, ElementSet.PavedElmSet),
                                                       (p, v) => p.InitialStreetStorage = v, "Paved Initial Street Storage");

            SetInitialConditionsForQuantity<UnpavedData>(model, outputTime,
                GetOutputCoverageByEngineParameters(model, QuantityType.Storage_mm, ElementSet.UnpavedElmSet),
                (u, v) => u.InitialLandStorage = v, "Unpaved Initial Land Storage");

            SetInitialConditionsForQuantity<UnpavedData>(model, outputTime,
                GetOutputCoverageByEngineParameters(model, QuantityType.GroundwaterLevel, ElementSet.UnpavedElmSet),
                                                         (u, v) => u.InitialGroundWaterLevelConstant = v, "Unpaved Initial Groundwater Level");
            
            //greenhouse doesn't expose mm (only m3..) bleh
            SetInitialConditionsForQuantity<GreenhouseData>(model, outputTime,
                GetOutputCoverageByEngineParameters(model, QuantityType.Storage_m3, ElementSet.GreenhouseElmSet),
                                                            (u, v) => u.InitialRoofStorage = v, "Greenhouse Initial Roof Storage",
                                                            (c, v) => FromCubicToMm(v, c.CalculationArea));
            return true;
        }

        /// <summary>
        /// given a calculation area (m2), convert from m3 to mm
        /// </summary>
        /// <param name="cubicM"></param>
        /// <param name="calculationArea"></param>
        /// <returns></returns>
        private static double FromCubicToMm(double cubicM, double calculationArea)
        {
            return (cubicM/calculationArea)*1000.0;
        }

        private static void SetInitialConditionsForQuantity<T>(RainfallRunoffModel model, DateTime outputTime,
                                                               IFeatureCoverage sourceCoverage,
                                                               Action<T, double> propertySetter, string conditionName,
                                                               Func<CatchmentModelData, double, double> conversion =
                                                                   null) where T : CatchmentModelData
        {
            var catchmentDatas = model.ModelData.OfType<T>().ToList();
            conversion = conversion ?? ((c, v) => v);

            if (sourceCoverage == null)
            {
                if (catchmentDatas.Count > 0)
                {
                    log.WarnFormat("Cannot take initial condition '{0}' from output; output not available",
                                   conditionName);
                }
                return;
            }

            var catchments = sourceCoverage.Features.OfType<Catchment>().ToList();
            
            foreach (var catchmentData in catchmentDatas)
            {
                var catchment = catchments.FirstOrDefault(f => f.Name == catchmentData.Name);
                if (catchment != null)
                {
                    propertySetter(catchmentData, conversion(catchmentData, (double)sourceCoverage[outputTime, catchment]));
                }
            }
        }

        private static IFeatureCoverage GetOutputCoverageByEngineParameters(RainfallRunoffModel model, QuantityType quantityType, ElementSet elementSet)
        {
            var engineParam = model.OutputSettings.GetEngineParameter(quantityType, elementSet);
            return (IFeatureCoverage)model.OutputCoverages.FirstOrDefault(c => c.Components[0].Name == engineParam.Name);
        }
    }
}