using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using GeoAPI.Geometries;
using log4net;
using System;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwSurfaceData: INwrwFeature
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwSurfaceData));
        public string Name { get; set; } // AFV_IDE
        public string MeteoStationId { get; set; } // NSL_DEF
        public string RunoffDefinitionFile  { get; set; } // AFV_DEF, we only support nwrw.csv for now.
        public NwrwSurfaceType NwrwSurfaceType { get; set; } // AFV_IDE
        public double SurfaceArea { get; set; } // AFV_OPP
        public string Remark { get; set; } // ALG_TOE



        public void SetGeometry(NwrwData nwrwData, IGeometry geometry)
        {
            if (nwrwData == null)
            {
                return;
            }

            nwrwData.Catchment.Geometry = geometry;

            var area = nwrwData.SurfaceLevelDict.Values.Sum();
            if (area > 0)
                nwrwData.Catchment.SetAreaSize(area);
            nwrwData.CalculationArea = nwrwData.Catchment.AreaSize;
        }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null)
            {
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

            nwrwData.MeteoStationId = this.MeteoStationId;

            if (!nwrwData.SurfaceLevelDict.ContainsKey(this.NwrwSurfaceType))
            {
                nwrwData.SurfaceLevelDict.Add(this.NwrwSurfaceType, SurfaceArea);
            }
            else
            {
                nwrwData.SurfaceLevelDict[this.NwrwSurfaceType] = this.SurfaceArea;
            }
        }
    }
}
