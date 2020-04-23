using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
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

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model, NwrwImporterHelper helper)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null) throw new ArgumentException();

            if (Geometry == null)
            {
                Log.Warn(
                    $"Could not add {DryWeatherFlowId} to {Name}, because the geometry of the catchment is not set.");
                return;
            }

            if (DischargeType == DischargeType.Lateral)
            {
                return;
            } // handled in the importer, requires FM knowledge

            if (!rrModel.NwrwDryWeatherFlowDefinitions.Any(dwfd => dwfd.Name.Equals(DryWeatherFlowId)))
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
        /// Sets the correct value for the lateral source property with the correct units.
        /// If the NwrwDischargeData FOM has a DryWeatherFlowId, get the lateral source from
        /// the corresponding definition. Else, we assume the lateral source was read directly
        /// from Debiet.csv.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the lateral surface could not be set.</exception>
        public void SetCorrectLateralSurface(
            ILookup<string, NwrwDryWeatherFlowDefinition> nwrwDryWeatherFlowDefinitionbyName)
        {
            if (nwrwDryWeatherFlowDefinitionbyName == null)
                throw new ArgumentNullException(nameof(nwrwDryWeatherFlowDefinitionbyName));


            if (!string.IsNullOrWhiteSpace(DryWeatherFlowId) &&
                !nwrwDryWeatherFlowDefinitionbyName.Contains(DryWeatherFlowId))
            {
                Log.Warn($"Cannot find NwrwDryWeatherFlowDefinition in RR model by name: {DryWeatherFlowId}");
            }



            if (string.IsNullOrWhiteSpace(DryWeatherFlowId) ||
                !nwrwDryWeatherFlowDefinitionbyName.Contains(DryWeatherFlowId))
            {
                LateralSurface /= 86400; // from m³/day to m³/s
            }
            else
            {
                foreach (var dwf in nwrwDryWeatherFlowDefinitionbyName[DryWeatherFlowId])
                {
                    LateralSurface = dwf.DailyVolumeConstant / 1000 / 3600; // from dm³/day to m³/s 
                }
            }
        }
    }

}
