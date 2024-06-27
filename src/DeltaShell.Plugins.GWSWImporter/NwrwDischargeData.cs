using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// File Object Model for Gwsw debiet.csv.
    /// </summary>
    public class NwrwDischargeData : INwrwFeature
    {
        private const NwrwSurfaceType SpecialCaseSurfaceType = NwrwSurfaceType.ClosedPavedWithSlope;
        public string Name { get; set; } // UNI_IDE
        public DischargeType DischargeType { get; set; } // DEB_TYPE
        public string DryWeatherFlowId { get; set; } // VER_IDE
        public int NumberOfPeople { get; set; } // AVV_ENH
        public double LateralSurface { get; set; } // AFV_OPP
        public string Remark { get; set; } // ALG_TOE

        public IGeometry Geometry { get; set; }

        public void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper, ILogHandler logHandler)
        {
            if (rrModel == null)
            {
                logHandler?.ReportError("Cannot add NWRW catchment if model is not provided");
                return;
            }

            if (Geometry == null)
            {
                logHandler?.ReportWarning(
                    $"Could not add {DryWeatherFlowId} to {Name}, because the geometry of the catchment is not set.");
                return;
            }

            if (!IsSpecialCase() && DischargeType == DischargeType.Lateral)
            {
                return;
            } // handled in the importer, requires FM knowledge

            if (!rrModel.NwrwDryWeatherFlowDefinitions.Any(dwfd => dwfd.Name.Equals(DryWeatherFlowId)) &&
                !IsSpecialCase())
            {
                logHandler?.ReportWarning($"Could not add '{DryWeatherFlowId}' to {Name}. No definition found for {DryWeatherFlowId}.");
                return;
            }

            if (!helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId.ContainsKey(Name))
            {
                NwrwData.CreateNewNwrwDataWithCatchment(rrModel, Name, helper, logHandler);
            }
        }

        public void InitializeNwrwCatchmentModelData(NwrwData nwrwData)
        {
            if (IsSpecialCase())
            {
                if (!nwrwData.SurfaceLevelDict.ContainsKey(SpecialCaseSurfaceType))
                {
                    nwrwData.SurfaceLevelDict.Add(SpecialCaseSurfaceType, LateralSurface);
                }
                else
                {
                    nwrwData.SurfaceLevelDict[SpecialCaseSurfaceType] += LateralSurface;
                }
            }
            else
            {
                AddDryWeatherFlowToNwrwCatchment(nwrwData);
            }

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
        /// In case the DischargeType is 'LAT' and the SurfaceLevel is 0 (so, not a special case),
        /// we have to create a lateral on the FM model with a flow in m³/s. In Verloop.csv the flow
        /// is given in dm³/day. Here we convert the flow to the correct units.
        /// </summary>
        /// <returns>Lateral flow in m³/s.</returns>
        /// <exception cref="ArgumentException">Thrown when the lateral surface could not be set.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the required dryweather flow definition cannot be found.</exception>
        public double CalculateLateralFlow(ILookup<string, NwrwDryWeatherFlowDefinition> nwrwDryWeatherFlowDefinitionByName, ILogHandler logHandler)
        {
            if (nwrwDryWeatherFlowDefinitionByName == null)
            {
                logHandler?.ReportError(Resources.NwrwDischargeData_CalculateLateralFlow_In_CalculateLateralFlow_parameter_nwrwDryWeatherFlowDefinitionByName_is_null);
                return double.NaN;
            }

            if (!string.IsNullOrWhiteSpace(DryWeatherFlowId) &&
                !nwrwDryWeatherFlowDefinitionByName.Contains(DryWeatherFlowId))
            {
                logHandler?.ReportError(string.Format(Resources.NwrwDischargeData_CalculateLateralFlow_Cannot_find_NwrwDryWeatherFlowDefinition_in_RR_model_by_name___0, DryWeatherFlowId));
                return double.NaN;
            }

            double lateralFlow = 0;
            foreach (var dwf in nwrwDryWeatherFlowDefinitionByName[DryWeatherFlowId])
            {
                lateralFlow = dwf.DailyVolumeConstant / 1000 / 3600; // from dm³/day to m³/s 
            }

            return lateralFlow;
        }

        // FM1D2D-861
        public bool IsSpecialCase()
        {
            return DischargeType == DischargeType.Lateral &&
                   string.IsNullOrWhiteSpace(DryWeatherFlowId) &&
                   LateralSurface > 0;
        }
    }
}
