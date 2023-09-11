using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// NwrwData contains nwrw catchment data from oppervlak.csv and/or debiet.csv.
    /// </summary>
    /// <seealso cref="CatchmentModelData" />
    /// <seealso cref="INwrwFeature" />
    [Entity(FireOnCollectionChange = false)]
    public class NwrwData : CatchmentModelData
    {
        //nhib
        public NwrwData(): base(null) { }

        public NwrwData(Catchment catchment) : base(catchment)
        {
            catchment.ModelData = this;
            NodeOrBranchId = Name;
            DryWeatherFlows.Add(new DryWeatherFlow(NwrwDryWeatherFlowDefinition.DefaultDwaId));
            DryWeatherFlows.Add(new DryWeatherFlow(NwrwDryWeatherFlowDefinition.DefaultDwaId));
        }


        public string NodeOrBranchId { get; set; } // UNI_IDE (debiet.csv or oppervlak.csv)

        /// <summary>
        /// DryWeatherFlow[0] = DWF definition (inhabitant)
        /// DryWeatherFlow[1] = DWF definition (company)
        /// multiple are possible in GWSW & SOBEK2 but not supported by us.
        /// GWSW: VER_IDE and AVV_ENH (debiet.csv)
        /// </summary>
        public IList<DryWeatherFlow> DryWeatherFlows { get; set; } = new List<DryWeatherFlow>(); 
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

            CalculationArea = Catchment.GeometryArea;
        }

        public static Dictionary<string, NwrwData> CreateNewNwrwDataAndCatchments(IHydroModel model, string[] names)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null)
            {
                throw new ArgumentNullException(nameof(model), "Can not add Nwrw catchment without a model.");
            }

            var catchments = names.Select(n =>
            {
                var catchment = Catchment.CreateDefault();
                catchment.CatchmentType = CatchmentType.NWRW;
                catchment.Name = n;
                return catchment;
            }).ToArray();
            
            rrModel.Basin.Catchments.AddRange(catchments);

            var nwrwDataLookup = rrModel.GetAllModelData()
                                        .OfType<NwrwData>()
                                        .ToDictionary(d => d.Catchment.Name, StringComparer.InvariantCultureIgnoreCase);

            foreach (var catchment in catchments)
            {
                if (nwrwDataLookup.ContainsKey(catchment.Name))
                {
                    continue;
                }

                // add missing data
                var catchmentModelData = catchment.CreateDefaultModelData();
                if (catchmentModelData == null)
                    continue;

                rrModel.ModelData.Add(catchmentModelData);
                rrModel.FireModelDataAdded(catchmentModelData);
                nwrwDataLookup[catchment.Name] = catchmentModelData as NwrwData;
            }

            return nwrwDataLookup;
        }

        public static void CreateNewNwrwDataWithCatchment(IHydroModel model, string name, NwrwImporterHelper helper, ILogHandler logHandler)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null)
            {
                logHandler?.ReportError($"Can not add Nwrw catchment {name} without a model.");
                return;
            }

            if (helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId.ContainsKey(name))
            {
                return;
            }

            var catchment = Catchment.CreateDefault();
            catchment.CatchmentType = CatchmentType.NWRW;
            catchment.Name = name;
            CurrentNwrwCatchmentModelDataByNodeOrBranchId = helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId;
            rrModel.ModelDataAdded += RrModelOnModelDataAdded; 
            rrModel.Basin.Catchments.Add(catchment);
            rrModel.ModelDataAdded -= RrModelOnModelDataAdded;
            
            CurrentNwrwCatchmentModelDataByNodeOrBranchId = null;
        }

        private static ConcurrentDictionary<string, NwrwData> CurrentNwrwCatchmentModelDataByNodeOrBranchId { get; set; }

        private static void RrModelOnModelDataAdded(object sender, EventArgs<CatchmentModelData> eventArgs)
        {
            if (!(eventArgs.Value is NwrwData nwrwData)) 
                return;

            CurrentNwrwCatchmentModelDataByNodeOrBranchId?.TryAdd(nwrwData.Name, nwrwData);
        }
    }
}
