using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using GeoAPI.Geometries;

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


        public IDictionary<NwrwSurfaceType, double> SurfaceLevelDict { get; set; } = new Dictionary<NwrwSurfaceType, double>();
        public string DryWeatherFlowId { get; set; }
        public string MeteoStationId { get; set; }
        public int NumberOfPeople { get; set; }


        // Pluvius only
        public int NumberOfSpecialAreas { get; set; }

        // Pluvius only
        public IList<NwrwSpecialArea> SpecialAreas { get; set; } = new List<NwrwSpecialArea>();


        public void SetGeometry(IGeometry geometry)
        {
            Catchment.Geometry = geometry;
            var area = SurfaceLevelDict.Values.Sum();
            if (area>0)
                Catchment.SetAreaSize(area);
            CalculationArea = Catchment.AreaSize;
        }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if(rrModel == null || rrModel.ModelData.Any(cmd => cmd.Name == this.Name)) return;

            if (!rrModel.Basin.Catchments.Contains(this.Catchment))
            {
                rrModel.Basin.Catchments.Add(this.Catchment);
            }

            if (rrModel.Basin.Catchments.Contains(this.Catchment) && !rrModel.ModelData.Contains(this))
            {
                rrModel.ModelData.Add(this);
                rrModel.FireModelDataAdded(this);
            }
        }
    }

}
