using DelftTools.Hydro;
using GeoAPI.Geometries;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public int NumberOfPeople { get; set; } // AVV_ENH
        public double LateralSurface { get; set; } // AFV_OPP
        public string Remark { get; set; } // ALG_TOE

        public IGeometry Geometry { get; set; }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null)
            {
                Log.Warn($"Could not add discharge data to {Name}.");
                return;
            }

            if (Geometry == null)
            {
                Log.Warn($"Could not add {DryWeatherFlowId} to {Name}, because the geometry of the catchment is not set.");
                return;
            }

            if (DischargeType == DischargeType.Lateral)
            {
                Log.Warn($"Could not add '{DryWeatherFlowId}' to {Name}. Discharge type '{nameof(DischargeType.Lateral)}' is not yet supported.");
                return;
            }

            if (!rrModel.NwrwDryWeatherFlowDefinitions.Any(dwfd => dwfd.Name.Equals(DryWeatherFlowId)))
            {
                Log.Warn($"Could not add '{DryWeatherFlowId}' to {Name}. No definition found for {DryWeatherFlowId}.");
                return;
            }

            var nwrwData = rrModel.GetAllModelData().OfType<NwrwData>().FirstOrDefault(md => md.NodeOrBranchId.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
            if (nwrwData == null)
            {
                nwrwData = NwrwData.CreateNewNwrwDataWithCatchment(rrModel, Name);
            }

            AddDryWeatherFlowToNwrwCatchment(nwrwData);

            nwrwData.LateralSurface = LateralSurface;

            nwrwData.Catchment.Geometry = Geometry;
            nwrwData.Catchment.IsGeometryDerivedFromAreaSize = true;

            nwrwData.UpdateCatchmentAreaSize();
        }

        private void AddDryWeatherFlowToNwrwCatchment(NwrwData nwrwData)
        {
            // Only two dry weather flow ids per catchment are supported.
            // See issue FM1D2D-535.
            IList<DryWeatherFlow> nwrwDataDryWeatherFlows = nwrwData.DryWeatherFlows;
            if (nwrwDataDryWeatherFlows.Count >= 2)
            {
                Log.Warn($"Could not add {DryWeatherFlowId} to {Name}. A maximum of two dry weather flow ids per catchment are currently supported.");
                return;
            }

            var dryweatherFlow = new DryWeatherFlow(DryWeatherFlowId)
            {
                NumberOfUnits = NumberOfPeople
            };

            if (nwrwDataDryWeatherFlows.Count == 1 
                && nwrwDataDryWeatherFlows[0].DryWeatherFlowId == NwrwData.DEFAULT_DWA_ID)
            {
                nwrwDataDryWeatherFlows[0] = dryweatherFlow;
            }
            else 
            {
                nwrwDataDryWeatherFlows.Add(dryweatherFlow);
            }
            
        }
    }
}
