using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils;
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
        private IDictionary<NwrwSurfaceType, double> surfaceLevelDict = new Dictionary<NwrwSurfaceType, double>();
        private IList<NwrwSpecialArea> specialAreas = new List<NwrwSpecialArea>();

        //nhib
        public NwrwData(): base(null) { }

        public NwrwData(Catchment catchment) : base(catchment){ }


        public IDictionary<NwrwSurfaceType, double> SurfaceLevelDict
        {
            get { return surfaceLevelDict; }
            set
            {
                surfaceLevelDict = value; 
            }
        }

        public string DryWeatherFlowId { get; set; }
        public string MeteoStationId { get; set; }
        public int NumberOfPeople { get; set; }
        public int NumberOfSpecialAreas { get; set; }

        public IList<NwrwSpecialArea> SpecialAreas
        {
            get { return specialAreas; }
            set { specialAreas = value; }
        }

        public double AreaAdjustmentFactor { get; set; }



        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if(rrModel == null || rrModel.ModelData.Any(cmd => cmd.Name == this.Name)) return;
            
            rrModel.ModelData.Add(this);
        }
    }

}
