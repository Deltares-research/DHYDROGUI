using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW
{
    public enum NWRWSurfaceType
    {
        [Description("GVH_HEL")]
        ClosedPavedWithSlope,
        [Description("GVH_VLA")]
        ClosedPavedFlat,
        [Description("GVH_VLU")]
        ClosedPavedFlatStretch,
        [Description("OVH_HEL")]
        OpenPavedWithSlope,
        [Description("OVH_VLA")]
        OpenPavedFlat,
        [Description("OVH_VLU")]
        OpenPavedFlatStretched,
        [Description("DAK_HEL")]
        RoofWithSlope,
        [Description("DAK_VLA")]
        RoofFlat,
        [Description("DAK_VLU")]
        RoofFlatStretched,
        [Description("ONV_HEL")]
        UnpavedWithSlope,
        [Description("ONV_VLA")]
        UnpavedFlat,
        [Description("ONV_VLU")]
        UnpavedFlatStretched
    }

    public class GwswNWRWGlobalData : INwrwFeature
    {
        public NWRWSurfaceType SurfaceType { get; set; }
        public double SurfaceStorage { get; set; }
        public double InfiltrationCapacityMax { get; set; }
        public double InfiltrationCapacityMin { get; set; }
        public double InfiltrationCapacityReduction { get; set; }
        public double InfiltrationCapacityRecovery { get; set; }
        public double RunoffDelay { get; set; }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null) return;
            if (!rrModel.NWRWGlobalDataDict.ContainsKey(this.SurfaceType))
                rrModel.NWRWGlobalDataDict.Add(this.SurfaceType, this);
        }
    }


    [Entity(FireOnCollectionChange = false)]
    public class NWRWData : CatchmentModelData, INwrwFeature
    {
        //nhib
        public NWRWData(): base(null) { }

        public NWRWData(Catchment catchment) : base(catchment){ }
        

        public Dictionary<NWRWSurfaceType, double> SurfaceLevelDict { get; set; }

        public string UniqueId { get; set; }
        public string DryWeatherFlowId { get; set; }
        public string MeteoStationId { get; set; }
        public int NumberOfPeople { get; set; }
        public int NumberOfSpecialAreas { get; set; }
        public IList<NWRWSpecialArea> SpecialAreas { get; set; }
        public double AreaAdjustmentFactor { get; set; }



        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if(rrModel == null) return;
            rrModel.NWRWData.Add(this);
            rrModel.ModelData.Add(this);
        }
    }

}
