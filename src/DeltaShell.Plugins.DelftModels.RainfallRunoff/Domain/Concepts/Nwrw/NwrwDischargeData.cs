using DelftTools.Hydro;
using GeoAPI.Geometries;
using log4net;
using System;
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

            var nwrwData = rrModel.ModelData?.OfType<NwrwData>()?.FirstOrDefault(md => md.NodeOrBranchId.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
            if (nwrwData == null)
            {
                nwrwData = NwrwData.CreateNewNwrwDataWithCatchment(rrModel, Name);
            }

            nwrwData.DischargeType = DischargeType;
            nwrwData.NumberOfPeople = NumberOfPeople;
            nwrwData.LateralSurface = LateralSurface;

            nwrwData.Catchment.Geometry = Geometry;
            nwrwData.Catchment.IsGeometryDerivedFromAreaSize = true;

            // Add the moment the kernel does not support multiple DWF definitions per catchment.
            // For now, we decided that a catchment can have 1 DWF with a name starting with 'Inwoner'
            // and 1 DWF statrting with 'Bedrijf'. This will be implemented in both the kernel and GUI.
            // See issue FM1D2D-535.
            if (DryWeatherFlowId.StartsWith(NwrwDryWeatherFlowDefinition.INHABITANT_DWF, StringComparison.InvariantCultureIgnoreCase) && 
                nwrwData.DryWeatherFlowIdInhabitant == null)
            {
                nwrwData.DryWeatherFlowIdInhabitant = DryWeatherFlowId;
            }
            else if (DryWeatherFlowId.StartsWith(NwrwDryWeatherFlowDefinition.COMPANY_DWF, StringComparison.InvariantCultureIgnoreCase) && 
                     nwrwData.DryWeatherFlowIdCompany == null)
            {
                nwrwData.DryWeatherFlowIdCompany = DryWeatherFlowId;
            }
            else
            {
                Log.Warn($"Could not add '{DryWeatherFlowId}' definition to '{Name}'.");
            }

            nwrwData.UpdateCatchmentAreaSize();
        }
    }
}
