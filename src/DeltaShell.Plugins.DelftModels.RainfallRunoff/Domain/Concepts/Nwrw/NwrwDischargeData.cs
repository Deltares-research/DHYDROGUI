using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using GeoAPI.Geometries;
using log4net;
using System;
using System.Linq;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// File Object Model for Gwsw debiet.csv.
    /// </summary>
    public class NwrwDischargeData : INwrwFeature
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwDischargeData));
        public string Name { get; set; } // UNI_IDE
        public DischargeType DischargeType { get; set; } // DEB_TYPE
        public string DryWeatherFlowId { get; set; } // VER_IDE
        public double NumberOfPeople { get; set; } // AVV_ENH
        public double LateralSurface { get; set; } // AFV_OPP
        public string Remark { get; set; } // ALG_TOE


        public void SetGeometry(NwrwData nwrwData, IGeometry geometry)
        {
            if (nwrwData == null)
            {
                return;
            }


            // Only set the geometry if the catchment does not yet contain any Surface
            if (nwrwData.SurfaceLevelDict.Count == 0)
            {
                nwrwData.Catchment.Geometry = geometry;
            }
            


            //If we only Discharge data and no Surface data, set the area to the
            //magic number 100 to make these catchments visible in the GUI.
            if (Math.Abs(nwrwData.CalculationArea) < 0.001)
            {
                nwrwData.Catchment.SetAreaSize(100);
                nwrwData.CalculationArea = nwrwData.Catchment.AreaSize;
            }
        }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null)
            {
                Log.Warn($"Could not add {nameof(NwrwDischargeData)} to {nameof(RainfallRunoffModel)}.");
                return;
            }

            if (this.DischargeType == DischargeType.Lateral)
            {
                Log.Warn($"Could not add {nameof(NwrwDischargeData)} to {nameof(RainfallRunoffModel)}. Discharge type {nameof(DischargeType.Lateral)} is not yet supported.");
                return;
            }

            if (!rrModel.NwrwDryWeatherFlowDefinitions.Any(dwfd => dwfd.Name.Equals(this.DryWeatherFlowId)))
            {
                Log.Warn($"Could not add {nameof(NwrwDischargeData)} to {nameof(RainfallRunoffModel)}. No definition found for {this.DryWeatherFlowId}.");
                return;
            }

            var nwrwData = rrModel.ModelData.OfType<NwrwData>().FirstOrDefault(md => md.NodeOrBranchId.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase));
            if (nwrwData == null)
            {
                var catchment = Catchment.CreateDefault();
                catchment.CatchmentType = CatchmentType.NWRW;
                catchment.Name = this.Name;
                rrModel.Basin.Catchments.Add(catchment);
                nwrwData = new NwrwData(catchment);
                nwrwData.NodeOrBranchId = this.Name;
                rrModel.ModelData.Add(nwrwData);
                rrModel.FireModelDataAdded(nwrwData);
            }

            nwrwData.DischargeType = this.DischargeType;
            nwrwData.NumberOfPeople = this.NumberOfPeople;
            nwrwData.LateralSurface = this.LateralSurface;

            // Add the moment the kernel does not support multiple DWF definitions per node/branch.
            // For now, we decided to check if the catchment already has a DWF definition.
            // If so, we do not add any other DWF definitions to the same catchment.
            if (nwrwData.DryWeatherFlowId != null)
            {
                Log.Warn($"Could not add {this.DryWeatherFlowId} definition. Multiple dry weather flow definitions per catchment are not yet supported.");
                return;
            }
            nwrwData.DryWeatherFlowId = this.DryWeatherFlowId;
        }
    }
}
