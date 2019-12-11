using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public enum NwrwSurfaceType
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


    [Entity(FireOnCollectionChange = false)]
    public class NwrwData : CatchmentModelData, INwrwFeature
    {
        //nhib
        public NwrwData(): base(null) { }

        public NwrwData(Catchment catchment) : base(catchment){ }
        

        public Dictionary<NwrwSurfaceType, double> SurfaceLevelDict { get; set; }
        public string DryWeatherFlowId { get; set; }
        public string MeteoStationId { get; set; }
        public int NumberOfPeople { get; set; }
        public int NumberOfSpecialAreas { get; set; }
        public IList<NwrwSpecialArea> SpecialAreas { get; set; }
        public double AreaAdjustmentFactor { get; set; }



        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if(rrModel == null || rrModel.ModelData.Any(cmd => cmd.Name == this.Name)) return;
            
            rrModel.ModelData.Add(this);
        }
    }

}
