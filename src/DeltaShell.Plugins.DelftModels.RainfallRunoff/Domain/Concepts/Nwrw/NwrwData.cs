using System;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using System.Collections.Generic;
using System.Linq;

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
        //nhib
        public NwrwData(): base(null) { }

        public NwrwData(Catchment catchment) : base(catchment){ }


        public string NodeOrBranchId { get; set; } // UNI_IDE (debiet.csv or oppervlak.csv)
        public DischargeType DischargeType { get; set; } // DEB_TYPE (debiet.csv)
        public string DryWeatherFlowIdInhabitant { get; set; } // VER_IDE (debiet.csv)
        public string DryWeatherFlowIdCompany { get; set; } // VER_IDE (debiet.csv)
        public IDictionary<NwrwSurfaceType, double> SurfaceLevelDict { get; set; } = new Dictionary<NwrwSurfaceType, double>(); // AFV_IDE and AFV_OPP (oppervlak.csv)
        public string MeteoStationId { get; set; } // NSL_STA (oppervlak.csv)
        public int NumberOfPeople { get; set; } // AVV_ENH (debiet.csv)
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
            else if (SurfaceLevelDict.Count == 0 && DischargeType != DischargeType.None && 
                     Catchment.IsGeometryDerivedFromAreaSize)
            {
                Catchment.SetAreaSize(100);
            }

            CalculationArea = Catchment.AreaSize;
        }

        public static NwrwData CreateNewNwrwDataWithCatchment(IHydroModel model, string name)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null)
            {
                throw new ArgumentNullException("Can not add Nwrw catchment without a model.");
            }

            var catchment = Catchment.CreateDefault();
            catchment.CatchmentType = CatchmentType.NWRW;
            catchment.Name = name;
            rrModel.Basin.Catchments.Add(catchment);

            var nwrwData = new NwrwData(catchment);
            nwrwData.NodeOrBranchId = name;
            rrModel.ModelData?.Add(nwrwData);
            rrModel.FireModelDataAdded(nwrwData);

            return nwrwData;
        }
    }

}
