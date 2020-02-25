using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.ImportExport.GWSW
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

            if (DischargeType == DischargeType.Lateral) {  return; } // handled in the importer, requires FM knowledge

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
            // First DWF definition must start with "inwoner", second must start with "bedrijf".
            // See issues FM1D2D-535 and FM1D2D-630.
            
            IList<DryWeatherFlow> nwrwDataDryWeatherFlows = nwrwData.DryWeatherFlows;

            var dryweatherFlow = new DryWeatherFlow(DryWeatherFlowId)
            {
                NumberOfUnits = NumberOfPeople
            };

            if (DryWeatherFlowId.StartsWith("Inwoner", StringComparison.InvariantCultureIgnoreCase))
            {
                nwrwDataDryWeatherFlows[0] = dryweatherFlow;
            }
            else if (DryWeatherFlowId.StartsWith("Bedrijf", StringComparison.InvariantCultureIgnoreCase))
            {
                nwrwDataDryWeatherFlows[1] = dryweatherFlow;
            }
        }

        /// <summary>
        /// Gets the LateralSurface value from the correct NwrwDryWeatherFlowDefinition.
        /// </summary>
        public void GetLateralSurfaceFromDefinition(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null)
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrWhiteSpace(DryWeatherFlowId)) return;

            NwrwDryWeatherFlowDefinition dwf = rrModel.NwrwDryWeatherFlowDefinitions.FirstOrDefault(dwfd => dwfd.Name.Equals(DryWeatherFlowId));
            LateralSurface = dwf.DailyVolumeConstant;
        }
    }
}
