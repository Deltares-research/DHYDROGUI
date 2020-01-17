using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using System.Collections.Generic;
using System.ComponentModel;

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

    public enum DischargeType
    {
        [Description("None")]
        None,

        [Description("VWD")]
        DryWeatherFlow,

        [Description("LAT")]
        Lateral
    }


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
        public string DryWeatherFlowId { get; set; } // VER_IDE (debiet.csv)
        public IDictionary<NwrwSurfaceType, double> SurfaceLevelDict { get; set; } = new Dictionary<NwrwSurfaceType, double>(); // AFV_IDE and AFV_OPP (oppervlak.csv)
        public string MeteoStationId { get; set; } // NSL_STA (oppervlak.csv)
        public double NumberOfPeople { get; set; } // AVV_ENH (debiet.csv)
        public double LateralSurface { get; set; } // AFV_OPP (debiet.csv, when DischargeType == 'LAT')

        public int NumberOfSpecialAreas { get; set; }
        public IList<NwrwSpecialArea> SpecialAreas { get; set; } = new List<NwrwSpecialArea>();


    }

}
