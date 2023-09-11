using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// File Object Model for Gwsw oppervlak.csv.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw.ANwrwFeature" />
    public class NwrwSurfaceData: ANwrwFeature
    {
        public NwrwSurfaceData(ILogHandler logHandler) : base(logHandler)
        {
        }
        public string MeteoStationId { get; set; } // NSL_DEF
        public string RunoffDefinitionFile  { get; set; } // AFV_DEF (we only support nwrw.csv for now)
        public NwrwSurfaceType NwrwSurfaceType { get; set; } // AFV_IDE
        public double SurfaceArea { get; set; } // AFV_OPP
        public string Remark { get; set; } // ALG_TOE
        
        public override void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper)
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

        public override void InitializeNwrwCatchmentModelData(NwrwData nwrwData)
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
