using System;
using System.Collections.Generic;
using System.Linq;
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
        private const NwrwSurfaceType SpecialCaseSurfaceType = NwrwSurfaceType.ClosedPavedWithSlope;

        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwDischargeData));


        public string Name { get; set; } // UNI_IDE
        public DischargeType DischargeType { get; set; } // DEB_TYPE
        public string DryWeatherFlowId { get; set; } // VER_IDE
        public int NumberOfPeople { get; set; } // AVV_ENH
        public double LateralSurface { get; set; } // AFV_OPP
        public string Remark { get; set; } // ALG_TOE

        public IGeometry Geometry { get; set; }


        public void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper)
        {
            if (rrModel == null) throw new ArgumentException();

            if (Geometry == null)
            {
                Log.Warn(
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
                Log.Warn($"Could not add '{DryWeatherFlowId}' to {Name}. No definition found for {DryWeatherFlowId}.");
                return;
            }

            if (!helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId.ContainsKey(Name))
            {
                NwrwData.CreateNewNwrwDataWithCatchment(rrModel, Name, helper);
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
        public double CalculateLateralFlow(
            ILookup<string, NwrwDryWeatherFlowDefinition> nwrwDryWeatherFlowDefinitionbyName)
        {
            if (nwrwDryWeatherFlowDefinitionbyName == null)
                throw new ArgumentNullException(nameof(nwrwDryWeatherFlowDefinitionbyName));

            if (!string.IsNullOrWhiteSpace(DryWeatherFlowId) &&
                !nwrwDryWeatherFlowDefinitionbyName.Contains(DryWeatherFlowId))
            {
                throw new InvalidOperationException($"Cannot find NwrwDryWeatherFlowDefinition in RR model by name: {DryWeatherFlowId}");
            }

            double lateralFlow = 0;
            foreach (var dwf in nwrwDryWeatherFlowDefinitionbyName[DryWeatherFlowId])
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
