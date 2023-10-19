using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DHYDRO.Common.Logging;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// File Object Model for Gwsw oppervlak.csv.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw.INwrwFeature" />
    public class NwrwSurfaceData: INwrwFeature
    {
        public string Name { get; set; } // UNI_IDE
        public string MeteoStationId { get; set; } // NSL_DEF
        public string RunoffDefinitionFile  { get; set; } // AFV_DEF (we only support nwrw.csv for now)
        public NwrwSurfaceType NwrwSurfaceType { get; set; } // AFV_IDE
        public double SurfaceArea { get; set; } // AFV_OPP
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
                    $"Could not add {NwrwSurfaceType} to {Name}, because the geometry of the catchment is not set.");
                return;
            }

            if (!helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId.ContainsKey(Name))
            {
                NwrwData.CreateNewNwrwDataWithCatchment(rrModel, Name, helper, logHandler);
            }
        }

        public void InitializeNwrwCatchmentModelData(NwrwData nwrwData)
        {
            nwrwData.MeteoStationId = MeteoStationId;
            nwrwData.Catchment.Geometry = Geometry;
            nwrwData.Catchment.IsGeometryDerivedFromAreaSize = true;

            if (!nwrwData.SurfaceLevelDict.ContainsKey(NwrwSurfaceType))
            {
                nwrwData.SurfaceLevelDict.Add(NwrwSurfaceType, SurfaceArea);
            }
            else
            {
                nwrwData.SurfaceLevelDict[NwrwSurfaceType] += SurfaceArea;
            }

            nwrwData.UpdateCatchmentAreaSize();
        }
    }
}
