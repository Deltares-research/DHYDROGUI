using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// NwrwData contains nwrw catchment data from oppervlak.csv and/or debiet.csv.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.CatchmentModelData" />
    /// <seealso cref="INwrwFeature" />
    [Entity(FireOnCollectionChange = false)]
    public class NwrwData : CatchmentModelData
    {
        public const string DEFAULT_DWA_ID = "Default_DWA";
        
        //nhib
        public NwrwData(): base(null) { }

        public NwrwData(Catchment catchment) : base(catchment)
        {
            NodeOrBranchId = Name;
            DryWeatherFlows.Add(new DryWeatherFlow(DEFAULT_DWA_ID));
            DryWeatherFlows.Add(new DryWeatherFlow(DEFAULT_DWA_ID));
        }


        public string NodeOrBranchId { get; set; } // UNI_IDE (debiet.csv or oppervlak.csv)
        public IList<DryWeatherFlow> DryWeatherFlows { get; set; } = new List<DryWeatherFlow>(); // VER_IDE and AVV_ENH (debiet.csv)
        public IDictionary<NwrwSurfaceType, double> SurfaceLevelDict { get; set; } = new Dictionary<NwrwSurfaceType, double>(); // AFV_IDE and AFV_OPP (oppervlak.csv)
        public string MeteoStationId { get; set; } // NSL_STA (oppervlak.csv)
        public double LateralSurface { get; set; } // AFV_OPP (debiet.csv, when DischargeType == 'LAT')
        public int NumberOfSpecialAreas { get; set; }
        public IList<NwrwSpecialArea> SpecialAreas { get; set; } = new List<NwrwSpecialArea>();


        public void UpdateCatchmentAreaSize()
        {
            var area = SurfaceLevelDict.Values.Sum();
            
            if (area > 0 && Catchment.IsGeometryDerivedFromAreaSize)
            {
                Catchment.SetAreaSize(area);
            }

            // If we only Discharge data and no Surface data, set the area to the
            // magic number 100 to make these catchments visible in the GUI.
            else if (SurfaceLevelDict.Count == 0 && Catchment.IsGeometryDerivedFromAreaSize)
            {
                Catchment.SetAreaSize(100);
            }

            CalculationArea = Catchment.AreaSize;
        }

        public static void CreateNewNwrwDataWithCatchment(IHydroModel model, string name, NwrwImporterHelper helper)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null)
            {
                throw new ArgumentNullException("Can not add Nwrw catchment without a model.");
            }

            var catchment = Catchment.CreateDefault();
            catchment.CatchmentType = CatchmentType.NWRW;
            catchment.Name = name;
            CurrentNwrwCatchmentModelDataByNodeOrBranchId = helper?.CurrentNwrwCatchmentModelDataByNodeOrBranchId;
            rrModel.ModelDataAdded += RrModelOnModelDataAdded; 
            rrModel.Basin.Catchments.Add(catchment);
            rrModel.ModelDataAdded -= RrModelOnModelDataAdded;
            CurrentNwrwCatchmentModelDataByNodeOrBranchId = null;
        }

        private static ConcurrentDictionary<string, NwrwData> CurrentNwrwCatchmentModelDataByNodeOrBranchId { get; set; }

        private static void RrModelOnModelDataAdded(object sender, EventArgs e)
        {
            var nwrwData = sender as NwrwData;
            if (nwrwData == null) return;
            CurrentNwrwCatchmentModelDataByNodeOrBranchId?.TryAdd(nwrwData.Name, nwrwData);
        }
    }
}
